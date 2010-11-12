using System;
using System.Data.Services.Client;
using System.Linq;

namespace NuGet {
    public class DataServicePackageRepository : PackageRepositoryBase {
        private readonly IDataServiceContext _context;
        private readonly IHashProvider _hashProvider;
        private readonly PackageDownloader _packageDownloader;

        public DataServicePackageRepository(Uri serviceRoot)
            : this(new DataServiceContextWrapper(serviceRoot), new CryptoHashProvider(), new PackageDownloader(new HttpClient())) {
        }

        public DataServicePackageRepository(IDataServiceContext context, IHashProvider hashProvider, PackageDownloader packageDownloader) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            if (hashProvider == null) {
                throw new ArgumentNullException("hashProvider");
            }

            _context = context;
            _hashProvider = hashProvider;
            _packageDownloader = packageDownloader;

            _context.SendingRequest += OnSendingRequest;
            _context.ReadingEntity += OnReadingEntity;
            _context.IgnoreMissingProperties = true;
        }

        private void OnReadingEntity(object sender, ReadingWritingEntityEventArgs e) {
            var package = (DataServicePackage)e.Entity;
            package.InitializeDownloader(DownloadAndVerifyPackage(package));
        }

        private Func<IPackage> DownloadAndVerifyPackage(DataServicePackage package) {
            if (!String.IsNullOrEmpty(package.PackageHash)) {
                byte[] hashBytes = Convert.FromBase64String(package.PackageHash);
                return () => _packageDownloader.DownloadPackage(_context.GetReadStreamUri(package), hashBytes, useCache: true);
            }
            else {
                // REVIEW: This is the only way (I know) to download the package on demand
                // GetReadStreamUri cannot be evaluated inside of OnReadingEntity. Lazily evaluate it inside DownloadPackage
                return () => _packageDownloader.DownloadPackage(_context.GetReadStreamUri(package));
            }
        }

        private void OnSendingRequest(object sender, SendingRequestEventArgs e) {
            // Initialize the request
            _packageDownloader.InitializeRequest(e.Request);
        }

        public override IQueryable<IPackage> GetPackages() {
            // REVIEW: Is it ok to assume that the package entity set is called packages?
            return new SmartDataServiceQuery<DataServicePackage>(_context, Constants.PackageServiceEntitySetName);
        }
    }
}
