using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using NExifTool.Writer;


namespace NExifTool.Tests
{
    public class WriteTests
    {
        const string SRC_FILE = "space test.jpg";

        // japanese string taken from here: https://stackoverflow.com/questions/2627891/does-process-startinfo-arguments-support-a-utf-8-string
        const string COMMENT = "this is a これはテストです test";

        readonly List<Operation> UPDATES = new List<Operation> {
            new SetOperation(new Tag("comment", COMMENT)),
            new SetOperation(new Tag("keywords", new string[] { "first", "second", "third", "hello world" }))
        };


        [Fact]
        public async void StreamToStreamWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);
            var src = new FileStream(SRC_FILE, FileMode.Open);

            var result = await et.WriteTagsAsync(src, UPDATES);

            Assert.True(result.Success);
            Assert.NotNull(result.Output);

            ValidateTags(await et.GetTagsAsync(result.Output));
        }


        [Fact]
        public async void StreamToFileWriteTest()
        {
            var testfile = "stream_to_file_test.jpg";
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);
            var src = new FileStream(SRC_FILE, FileMode.Open);

            var result = await et.WriteTagsAsync(src, UPDATES, testfile);

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync(testfile));

            File.Delete(testfile);
        }


        [Fact]
        public async void FileToStreamWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.WriteTagsAsync(SRC_FILE, UPDATES);

            Assert.True(result.Success);
            Assert.NotNull(result.Output);

            ValidateTags(await et.GetTagsAsync(result.Output));
        }


        [Fact]
        public async void FileToFileWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.WriteTagsAsync(SRC_FILE, UPDATES, "file_to_file_test.jpg");

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync("file_to_file_test.jpg"));

            File.Delete("file_to_file_test.jpg");
        }


        [Fact]
        public async void OverwriteTest()
        {
            File.Copy(SRC_FILE, "overwrite_test.jpg", true);

            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.OverwriteTagsAsync("overwrite_test.jpg", UPDATES);

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync("overwrite_test.jpg"));

            File.Delete("overwrite_test.jpg");
        }


        void ValidateTags(IEnumerable<Tag> tags)
        {
            var commentTag = tags.SingleOrDefault(x => string.Equals(x.Name, "comment", StringComparison.OrdinalIgnoreCase));
            var keywordsTag = tags.SinglePrimaryTag("keywords");

            Assert.NotNull(commentTag);
            Assert.Equal(COMMENT.Replace("\"", string.Empty), commentTag.Value);

            Assert.NotNull(keywordsTag);
            Assert.Equal(4, keywordsTag.List.Count);
            Assert.Equal("first", keywordsTag.List[0]);
            Assert.Equal("hello world", keywordsTag.List[3]);
        }
    }
}
