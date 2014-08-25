using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squirrel.Tests.TestHelpers;
using Xunit;

namespace Squirrel.Tests
{
    public class ActionFileDownloader : IFileDownloader
    {
        Func<string, string, Task> downloadFile;
        Func<string, Task<byte[]>> downloadUrl;

        public ActionFileDownloader(Func<string, string, Task> downloadFile, Func<string, Task<byte[]>> downloadUrl)
        {
            this.downloadFile = downloadFile;
            this.downloadUrl = downloadUrl;
        }

        public async Task DownloadFile(string url, string targetFile)
        {
            if (downloadFile != null) {
                await downloadFile(url, targetFile);
                return;
            }

            throw new NotImplementedException();
        }

        public async Task<byte[]> DownloadUrl(string url)
        {
            if (downloadUrl != null) {
                return await downloadUrl(url);
            }

            throw new NotImplementedException();
        }
    }

    public class CheckForUpdateTests
    {
        [Fact]
        public async Task CorruptedReleaseFileMeansWeStartFromScratch()
        {
            string tempDir;
            using (Utility.WithTempDirectory(out tempDir)) {
                var remotePackageDir = Directory.CreateDirectory(Path.Combine(tempDir, "remotePackages"));
                var localAppDir = Path.Combine(tempDir, "theApp");
                string localPackagesDir = Path.Combine(tempDir, "theApp", "packages");
                string localReleasesFile = Path.Combine(localPackagesDir, "RELEASES");

                new[] {
                    "Squirrel.Core.1.0.0.0-full.nupkg",
                }.ForEach(x => File.Copy(IntegrationTestHelper.GetPath("fixtures", x), Path.Combine(remotePackageDir.FullName, x)));

                using (var fixture = new UpdateManager(remotePackageDir.FullName, "theApp", FrameworkVersion.Net45, tempDir)) {
                    await fixture.FullInstall();
                }

                new[] {
                    "Squirrel.Core.1.1.0.0-full.nupkg",
                }.ForEach(x => File.Copy(IntegrationTestHelper.GetPath("fixtures", x), Path.Combine(remotePackageDir.FullName, x)));

                ReleaseEntry.WriteReleaseFile(ReleaseEntry.BuildReleasesFile(remotePackageDir.FullName), Path.Combine(remotePackageDir.FullName, "RELEASES"));

                File.WriteAllText(localReleasesFile, "lol not right");

                using (var fixture = new UpdateManager(remotePackageDir.FullName, "theApp", FrameworkVersion.Net45, tempDir)) {
                    await fixture.UpdateApp();
                }

                File.ReadAllLines(Path.Combine(localPackagesDir, "RELEASES")).Count().ShouldEqual(1);
                File.Exists(Path.Combine(localAppDir, "app-1.1.0.0", "NSync.Core.dll")).ShouldBeTrue();
            }


            /*
            string localPackagesDir = Path.Combine(".", "theApp", "packages");
            string localReleasesFile = Path.Combine(localPackagesDir, "RELEASES");

            var fileInfo = new Mock<FileInfoBase>();
            fileInfo.Setup(x => x.Exists).Returns(true);
            fileInfo.Setup(x => x.OpenRead())
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("lol this isn't right")));

            var dirInfo = new Mock<DirectoryInfoBase>();
            dirInfo.Setup(x => x.Exists).Returns(true);

            var fs = new Mock<IFileSystemFactory>();
            fs.Setup(x => x.GetFileInfo(localReleasesFile)).Returns(fileInfo.Object);
            fs.Setup(x => x.CreateDirectoryRecursive(localPackagesDir)).Verifiable();
            fs.Setup(x => x.DeleteDirectoryRecursive(localPackagesDir)).Verifiable();
            fs.Setup(x => x.GetDirectoryInfo(localPackagesDir)).Returns(dirInfo.Object);

            var urlDownloader = new Mock<IUrlDownloader>();
            var dlPath = IntegrationTestHelper.GetPath("fixtures", "RELEASES-OnePointOne");
            urlDownloader.Setup(x => x.DownloadUrl(It.IsAny<string>(), It.IsAny<IObserver<int>>()))
                .Returns(Observable.Return(File.ReadAllText(dlPath, Encoding.UTF8)));

            var fixture = new UpdateManager("http://lol", "theApp", FrameworkVersion.Net40, ".", fs.Object, urlDownloader.Object);
            using (fixture) {
                fixture.CheckForUpdate().First();
            }

            fs.Verify(x => x.CreateDirectoryRecursive(localPackagesDir), Times.Once());
            fs.Verify(x => x.DeleteDirectoryRecursive(localPackagesDir), Times.Once());
            */
        }

        [Fact]
        public void CorruptRemoteFileShouldThrowOnCheck()
        {
            Assert.False(true, "Rewrite this to be an integration test");

            /*
            string localPackagesDir = Path.Combine(".", "theApp", "packages");
            string localReleasesFile = Path.Combine(localPackagesDir, "RELEASES");

            var fileInfo = new Mock<FileInfoBase>();
            fileInfo.Setup(x => x.Exists).Returns(false);

            var dirInfo = new Mock<DirectoryInfoBase>();
            dirInfo.Setup(x => x.Exists).Returns(true);

            var fs = new Mock<IFileSystemFactory>();
            fs.Setup(x => x.GetFileInfo(localReleasesFile)).Returns(fileInfo.Object);
            fs.Setup(x => x.CreateDirectoryRecursive(localPackagesDir)).Verifiable();
            fs.Setup(x => x.DeleteDirectoryRecursive(localPackagesDir)).Verifiable();
            fs.Setup(x => x.GetDirectoryInfo(localPackagesDir)).Returns(dirInfo.Object);

            var urlDownloader = new Mock<IUrlDownloader>();
            urlDownloader.Setup(x => x.DownloadUrl(It.IsAny<string>(), It.IsAny<IObserver<int>>()))
                .Returns(Observable.Return("lol this isn't right"));

            var fixture = new UpdateManager("http://lol", "theApp", FrameworkVersion.Net40, ".", fs.Object, urlDownloader.Object);

            using (fixture) {
                Assert.Throws<Exception>(() => fixture.CheckForUpdate().First());   
            }
            */
        }

        [Fact(Skip = "TODO")]
        public void IfLocalVersionGreaterThanRemoteWeRollback()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "TODO")]
        public void IfLocalAndRemoteAreEqualThenDoNothing()
        {
            throw new NotImplementedException();
        }
    }
}