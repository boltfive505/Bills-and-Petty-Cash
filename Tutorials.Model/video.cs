namespace Tutorials.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("video")]
    public partial class video
    {
        public long Id { get; set; }

        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public long CategoryId { get; set; }

        public string EntryLevel { get; set; }

        public string VideoFile { get; set; }

        public string ThumbnailFile { get; set; }

        public string FileAttachment { get; set; }

        public string YoutubeUrlLink { get; set; }

        public string RawVideoFile1 { get; set; }

        public string RawVideoFile2 { get; set; }

        public string RawVideoFile3 { get; set; }

        public string PeriodType { get; set; }

        public int? PeriodDay { get; set; }

        public string PeriodName { get; set; }

        public int IsPeriodNotApplicable { get; set; } = 1;

        public virtual category category { get; set; }
    }
}
