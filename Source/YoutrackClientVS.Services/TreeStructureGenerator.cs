﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using ParseDiff;
using YouTrackClientVS.Contracts.Interfaces;
using YouTrackClientVS.Contracts.Models.GitClientModels;
using YouTrackClientVS.Contracts.Models.Tree;

namespace YouTrackClientVS.Services
{
    [Export(typeof(ITreeStructureGenerator))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TreeStructureGenerator : ITreeStructureGenerator
    {
        public IEnumerable<ICommentTree> CreateCommentTree(IEnumerable<GitComment> gitComments, char separator = '/')
        {
            var searchableGitComments = gitComments.ToDictionary(comment => comment.Id);

            var result = new Dictionary<int, List<ObjectTree>>();

            var maxLevel = -1;
            foreach (var comment in gitComments)
            {
                var level = 0;
                var ids = new List<long>();
                var path = new StringBuilder();

                ids.Add(comment.Id);
                var tmpComment = comment;
                while (tmpComment.Parent != null)
                {
                    ids.Add(tmpComment.Parent.Id);
                    level++;

                    tmpComment = searchableGitComments[tmpComment.Parent.Id];
                }

                if (!result.ContainsKey(level))
                {
                    result[level] = new List<ObjectTree>();
                    if (level > maxLevel) maxLevel = level;
                }

                for (var pathIndex = ids.Count - 1; pathIndex > -1; pathIndex -= 1)
                {
                    path.Append(ids[pathIndex]);

                    if (pathIndex > 0) path.Append(separator);
                }

                result[level].Add(new ObjectTree(path.ToString(), new GitComment()
                {
                    Content = comment.Content,
                    CreatedOn = comment.CreatedOn,
                    Id = comment.Id,
                    Parent = comment.Parent,
                    User = comment.User,
                    UpdatedOn = comment.UpdatedOn,
                    Inline = comment.Inline,
                    IsDeleted = comment.IsDeleted,
                    Version = comment.Version
                }));
            }


            ICommentTree entryComment = new CommentTree(null);
            for (var i = 0; i <= maxLevel; i++)
            {
                var preparedComments = result[i];
                foreach (var objectTree in preparedComments)
                {
                    var currentComment = entryComment;
                    var pathChunks = objectTree.Path.Split(separator);
                    foreach (var pathChunk in pathChunks)
                    {
                        var tmp = currentComment.Comments.Where(x => x.Comment.Id.Equals(long.Parse(pathChunk)));
                        if (tmp.Any())
                        {
                            currentComment = tmp.Single();
                        }
                        else
                        {
                            ICommentTree newItem = new CommentTree(objectTree.GitComment);
                            currentComment.Comments.Add(newItem);
                            currentComment = newItem;
                        }
                    }
                }
            }

            return entryComment.Comments;
        }

        public IEnumerable<ITreeFile> CreateFileTree(IEnumerable<FileDiff> fileDiffs, string rootFileName = "test",
            char separator = '/')
        {
            var entryFile = new TreeDirectory(rootFileName);

            foreach (var fileDiff in fileDiffs.Where(x => !string.IsNullOrEmpty(x.DisplayFileName.Trim())))
            {
                ITreeFile currentFile = entryFile;

                var pathChunks = fileDiff.DisplayFileName.Split(separator);
                var lastItem = pathChunks.Last();
                foreach (var pathChunk in pathChunks)
                {
                    var tmp = currentFile.Files.Where(x => x.Name.Equals(pathChunk));
                    if (tmp.Any())
                    {
                        currentFile = tmp.Single();
                    }
                    else
                    {
                        ITreeFile newItem;
                        if (lastItem.Equals(pathChunk))
                            newItem = new TreeFile(pathChunk, fileDiff);
                        else
                            newItem = new TreeDirectory(pathChunk);

                        currentFile.Files.Add(newItem);
                        currentFile = newItem;
                    }
                }
            }

            ExpandTree(entryFile.Files);

            return entryFile.Files;
        }

        private void ExpandTree(IEnumerable<ITreeFile> files)
        {
            foreach (var treeFile in files)
            {
                if (treeFile.Files.Any())
                    ExpandTree(treeFile.Files);

                treeFile.IsExpanded = true;
            }
        }
    }
}