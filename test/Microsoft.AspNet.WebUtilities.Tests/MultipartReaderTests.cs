﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.WebUtilities
{
    public class MultipartReaderTests
    {
        private const string Boundary = "9051914041544843365972754266";
        private const string OnePartBody =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266--
";
        private const string OnePartBodyWithTrailingWhitespace =
@"--9051914041544843365972754266             
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266--
";
        // It's non-compliant but common to leave off the last CRLF.
        private const string OnePartBodyWithoutFinalCRLF =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266--";
        private const string TwoPartBody =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266
Content-Disposition: form-data; name=""file1""; filename=""a.txt""
Content-Type: text/plain

Content of a.txt.

--9051914041544843365972754266--
";
        private const string TwoPartBodyWithUnicodeFileName =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266
Content-Disposition: form-data; name=""file1""; filename=""a色.txt""
Content-Type: text/plain

Content of a.txt.

--9051914041544843365972754266--
";
        private const string ThreePartBody =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
--9051914041544843365972754266
Content-Disposition: form-data; name=""file1""; filename=""a.txt""
Content-Type: text/plain

Content of a.txt.

--9051914041544843365972754266
Content-Disposition: form-data; name=""file2""; filename=""a.html""
Content-Type: text/html

<!DOCTYPE html><title>Content of a.html.</title>

--9051914041544843365972754266--
";

        private static MemoryStream MakeStream(string text)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        [Fact]
        public async Task MutipartReader_ReadSinglePartBody_Success()
        {
            var stream = MakeStream(OnePartBody);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadSinglePartBodyWithTrailingWhitespace_Success()
        {
            var stream = MakeStream(OnePartBodyWithTrailingWhitespace);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadSinglePartBodyWithoutLastCRLF_Success()
        {
            var stream = MakeStream(OnePartBodyWithoutFinalCRLF);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadTwoPartBody_Success()
        {
            var stream = MakeStream(TwoPartBody);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(2, section.Headers.Count);
            Assert.Equal("form-data; name=\"file1\"; filename=\"a.txt\"", section.Headers["Content-Disposition"][0]);
            Assert.Equal("text/plain", section.Headers["Content-Type"][0]);
            buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("Content of a.txt.\r\n", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadTwoPartBodyWithUnicodeFileName_Success()
        {
            var stream = MakeStream(TwoPartBodyWithUnicodeFileName);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(2, section.Headers.Count);
            Assert.Equal("form-data; name=\"file1\"; filename=\"a色.txt\"", section.Headers["Content-Disposition"][0]);
            Assert.Equal("text/plain", section.Headers["Content-Type"][0]);
            buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("Content of a.txt.\r\n", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ThreePartBody_Success()
        {
            var stream = MakeStream(ThreePartBody);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(2, section.Headers.Count);
            Assert.Equal("form-data; name=\"file1\"; filename=\"a.txt\"", section.Headers["Content-Disposition"][0]);
            Assert.Equal("text/plain", section.Headers["Content-Type"][0]);
            buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("Content of a.txt.\r\n", Encoding.ASCII.GetString(buffer.ToArray()));

            section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(2, section.Headers.Count);
            Assert.Equal("form-data; name=\"file2\"; filename=\"a.html\"", section.Headers["Content-Disposition"][0]);
            Assert.Equal("text/html", section.Headers["Content-Type"][0]);
            buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("<!DOCTYPE html><title>Content of a.html.</title>\r\n", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadInvalidUtf8Header_ReplacementCharacters()
        {
            var body1 =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text"" filename=""a";

            var body2 =
@".txt""

text default
--9051914041544843365972754266--
";
            var stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(body1);
            stream.Write(bytes, 0, bytes.Length);

            // Write an invalid utf-8 segment in the middle
            stream.Write(new byte[] { 0xC1, 0x21 }, 0, 2);

            bytes = Encoding.UTF8.GetBytes(body2);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\" filename=\"a\uFFFD!.txt\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }

        [Fact]
        public async Task MutipartReader_ReadInvalidUtf8SurrogateHeader_ReplacementCharacters()
        {
            var body1 =
@"--9051914041544843365972754266
Content-Disposition: form-data; name=""text"" filename=""a";

            var body2 =
@".txt""

text default
--9051914041544843365972754266--
";
            var stream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(body1);
            stream.Write(bytes, 0, bytes.Length);

            // Write an invalid utf-8 segment in the middle
            stream.Write(new byte[] { 0xED, 0xA0, 85 }, 0, 3);

            bytes = Encoding.UTF8.GetBytes(body2);
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new MultipartReader(Boundary, stream);

            var section = await reader.ReadNextSectionAsync();
            Assert.NotNull(section);
            Assert.Equal(1, section.Headers.Count);
            Assert.Equal("form-data; name=\"text\" filename=\"a\uFFFDU.txt\"", section.Headers["Content-Disposition"][0]);
            var buffer = new MemoryStream();
            await section.Body.CopyToAsync(buffer);
            Assert.Equal("text default", Encoding.ASCII.GetString(buffer.ToArray()));

            Assert.Null(await reader.ReadNextSectionAsync());
        }
    }
}