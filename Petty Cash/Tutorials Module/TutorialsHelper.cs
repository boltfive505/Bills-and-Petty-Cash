using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Classes;
using Petty_Cash.Tutorials_Module.Objects;
using Tutorials.Model;
using System.Data.Entity;
using System.IO;

namespace Petty_Cash.Tutorials_Module
{
    public static class TutorialsHelper
    {
        public static string ThumbnailFilters = "Supported Files|*.png;*.jpg;*.jpeg";
        public static string VideoFilters = "Supported Files|*.mp4;*.avi;*.mov;*.flv";
        private static string[] _supportedThumbnailFormats = { ".png", ".jpg", ".jpeg" };
        private static string[] _supportedVideoFormats = { ".mp4", ".avi", ".mov", ".flv" };

        public const string TUTORIAL_VIDEO_FOLDER = "tutorials\\final videos";
        public const string TUTORIAL_THUMBNAIL_FOLDER = "tutorials\\thumbnails";
        public const string TUTORIAL_FILES_FOLDER = "tutorials\\files";
        public const string TUTORIAL_RAW_VIDEO_FOLDER = "tutorials\\raw videos";

        public static async Task<IEnumerable<TutorialCategoryViewModel>> GetCategoryListAsync()
        {
            using (var context = new TutorialsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from cat in context.category
                            select cat;
                var list = await query.ToListAsync();
                var categoryList = list.Select(i => new TutorialCategoryViewModel(i));
                return categoryList;
            }
        }

        public static void SetCategoryHierarchy(ref List<TutorialCategoryViewModel> categoryList)
        {
            foreach (var cat in categoryList)
            {
                //get parent
                if (cat.ParentCategoryId != null)
                    cat.ParentCategory = categoryList.FirstOrDefault(i => i.Id == cat.ParentCategoryId);
                //get children
                cat.HasCategoryChildren = categoryList.Any(i => i.ParentCategoryId != null && i.ParentCategoryId == cat.Id);
            }
            foreach (var cat in categoryList)
            {
                //get hierarchy level
                cat.HierarchyLevel = GetCategoryHierarchyLevel(cat);
                //set path name
                cat.PathName = GetCategoryPathName(cat);
                //set isactive hierarchy
                cat.IsActiveHierarchy = GetCategoryHierarchyIsActive(cat);
            }
        }

        public static void SetAllCategoryListVideoCounting(ref List<TutorialCategoryViewModel> categoryList, List<TutorialVideoViewModel> videoList)
        {
            foreach (var cat in categoryList)
            {
                int count = videoList.Count(i => i.Category.Id == cat.Id);
                cat.VideoCount = count == 0 ? null : (int?)count;
            }
        }

        private static int GetCategoryHierarchyLevel(TutorialCategoryViewModel categoryVm)
        {
            if (categoryVm.ParentCategory == null) return 0;
            return GetCategoryHierarchyLevel(categoryVm.ParentCategory) + 1;
        }

        private static string GetCategoryPathName(TutorialCategoryViewModel categoryVm)
        {
            string name = categoryVm.CategoryName;
            if (categoryVm.ParentCategory != null)
            {
                name = string.Format("{0}/{1}", GetCategoryPathName(categoryVm.ParentCategory), name);
            }
            return name;
        }

        private static bool GetCategoryHierarchyIsActive(TutorialCategoryViewModel categoryVm)
        {
            if (categoryVm.ParentCategory == null) return categoryVm.IsActive;
            if (!categoryVm.ParentCategory.IsActive)
                return false;
            else
                return GetCategoryHierarchyIsActive(categoryVm.ParentCategory);
        }

        public static async Task<IEnumerable<TutorialVideoViewModel>> GetVideoList()
        {
            using (var context = new TutorialsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from vid in context.video
                            join cat in context.category on vid.CategoryId equals cat.Id
                            select new
                            {
                                Video = vid,
                                Category = cat
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new TutorialVideoViewModel(i.Video, i.Category));
            }
        }

        public static async Task AddUpdateCategory(TutorialCategoryViewModel categoryVm)
        {
            using (var context = new TutorialsModel())
            {
                var category = await context.category.FirstOrDefaultAsync(i => i.Id == categoryVm.Id);
                if (category == null)
                {
                    category = new category();
                    context.category.Add(category);
                }
                category.CategoryName = categoryVm.CategoryName;
                category.Description = categoryVm.Description;
                category.IsActive = categoryVm.IsActive.ToLong();
                category.ParentCategoryId = categoryVm.ParentCategoryId;
                await context.SaveChangesAsync();

                categoryVm.Id = (int)category.Id;
            }
        }

        public static async Task AddUpdateVideo(TutorialVideoViewModel videoVm)
        {
            using (var context = new TutorialsModel())
            {
                var video = await context.video.FirstOrDefaultAsync(i => i.Id == videoVm.Id);
                if (video == null)
                {
                    video = new video();
                    context.video.Add(video);
                }
                video.LongDescription = videoVm.LongDescription;
                video.ShortDescription = videoVm.ShortDescription;
                video.CategoryId = videoVm.Category.Id;
                video.EntryLevel = videoVm.EntryLevel.ToString();
                video.YoutubeUrlLink = videoVm.YoutubeUrlLink;
                video.VideoFile = ProcessFile(video.VideoFile, videoVm.VideoFile, TutorialsHelper.TUTORIAL_VIDEO_FOLDER);
                video.ThumbnailFile = ProcessFile(video.ThumbnailFile, videoVm.ThumbnailFile, TutorialsHelper.TUTORIAL_THUMBNAIL_FOLDER);
                video.FileAttachment = ProcessFile(video.FileAttachment, videoVm.FileAttachment, TutorialsHelper.TUTORIAL_FILES_FOLDER);
                video.RawVideoFile1 = ProcessFile(video.RawVideoFile1, videoVm.RawVideoFile1, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                video.RawVideoFile2 = ProcessFile(video.RawVideoFile2, videoVm.RawVideoFile2, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                video.RawVideoFile3 = ProcessFile(video.RawVideoFile3, videoVm.RawVideoFile3, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                video.PeriodType = videoVm.PeriodType.ToString();
                video.PeriodDay = videoVm.PeriodDay;
                video.PeriodName = videoVm.PeriodName;
                video.IsPeriodNotApplicable = (int)videoVm.IsPeriodNotApplicable.ToLong();
                await context.SaveChangesAsync();

                videoVm.Id = (int)video.Id;
                videoVm.VideoFile = video.VideoFile;
                videoVm.ThumbnailFile = video.ThumbnailFile;
                videoVm.FileAttachment = video.FileAttachment;
                videoVm.RawVideoFile1 = video.RawVideoFile1;
                videoVm.RawVideoFile2 = video.RawVideoFile2;
                videoVm.RawVideoFile3 = video.RawVideoFile3;
            }
        }

        public static async Task DeleteVideo(TutorialVideoViewModel videoVm)
        {
            using (var context = new TutorialsModel())
            {
                var video = await context.video.FirstOrDefaultAsync(i => i.Id == videoVm.Id);
                if (video != null)
                {
                    //delete files
                    DeleteFile(video.VideoFile, TutorialsHelper.TUTORIAL_VIDEO_FOLDER);
                    DeleteFile(video.ThumbnailFile, TutorialsHelper.TUTORIAL_THUMBNAIL_FOLDER);
                    DeleteFile(video.FileAttachment, TutorialsHelper.TUTORIAL_FILES_FOLDER);
                    DeleteFile(video.RawVideoFile1, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                    DeleteFile(video.RawVideoFile2, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                    DeleteFile(video.RawVideoFile3, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
                    //delete from database
                    context.video.Remove(video);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static string ProcessFile(string oldFileName, string newFile, string subdirectory)
        {
            //check if file is changed
            if (oldFileName == newFile) return oldFileName;
            //upload new file, if exist
            string file = string.Empty;
            if (File.Exists(newFile))
            {
                file = FileHelper.Upload(newFile, subdirectory, "", FileHelper.FileNameRenameMode.NoRename);
            }
            //delete old file, if exist
            string oldFile = FileHelper.GetFile(oldFileName, subdirectory);
            if (File.Exists(oldFile))
                File.Delete(oldFile);
            return string.IsNullOrEmpty(file) ? string.Empty : Path.GetFileName(file);
        }

        private static void DeleteFile(string file, string subdirectory)
        {
            string oldFile = FileHelper.GetFile(file, subdirectory);
            if (File.Exists(oldFile))
                File.Delete(oldFile);
        }
    }
}
