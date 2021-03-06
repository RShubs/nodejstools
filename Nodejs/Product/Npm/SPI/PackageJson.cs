// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PackageJson : IPackageJson
    {
        private readonly string versionString;

        public PackageJson(dynamic package)
        {
            this.Keywords = LoadKeywords(package);
            this.Homepages = LoadHomepages(package);
            this.Files = LoadFiles(package);
            this.Dependencies = LoadDependencies(package);
            this.DevDependencies = LoadDevDependencies(package);
            this.BundledDependencies = LoadBundledDependencies(package);
            this.OptionalDependencies = LoadOptionalDependencies(package);
            this.AllDependencies = LoadAllDependencies(package);
            this.RequiredBy = LoadRequiredBy(package);

            this.Name = package.name?.ToString();
            this.versionString = package.version;
            this.Description = package.description?.ToString();
            this.Author = package.author == null ? null : Person.CreateFromJsonSource(package.author.ToString());
        }

        private static PackageJsonException WrapRuntimeBinderException(string errorProperty, RuntimeBinderException rbe)
        {
            return new PackageJsonException(
                string.Format(CultureInfo.CurrentCulture, @"Exception occurred retrieving {0} from package.json. The file may be invalid: you should edit it to correct an errors.

The following error occurred:

{1}",
                        errorProperty,
                        rbe));
        }

        private static IKeywords LoadKeywords(dynamic package)
        {
            try
            {
                return new Keywords(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("keywords", rbe);
            }
        }

        private static IHomepages LoadHomepages(dynamic package)
        {
            try
            {
                return new Homepages(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("homepage", rbe);
            }
        }

        private static IFiles LoadFiles(dynamic package)
        {
            try
            {
                return new PkgFiles(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("files", rbe);
            }
        }

        private static IDependencies LoadDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "dependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("dependencies", rbe);
            }
        }

        private static IDependencies LoadDevDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "devDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("dev dependencies", rbe);
            }
        }

        private static IBundledDependencies LoadBundledDependencies(dynamic package)
        {
            try
            {
                return new BundledDependencies(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("bundled dependencies", rbe);
            }
        }

        private static IDependencies LoadOptionalDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "optionalDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("optional dependencies", rbe);
            }
        }

        private static IDependencies LoadAllDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "dependencies", "devDependencies", "optionalDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("all dependencies", rbe);
            }
        }

        private static IEnumerable<string> LoadRequiredBy(dynamic package)
        {
            try
            {
                return (package["_requiredBy"] as IEnumerable<JToken> ?? Enumerable.Empty<JToken>()).Values<string>().ToList();
            }
            catch (RuntimeBinderException rbe)
            {
                System.Diagnostics.Debug.WriteLine(rbe);
                throw WrapRuntimeBinderException("required by", rbe);
            }
        }

        public string Name { get; }

        public SemverVersion Version
        {
            get
            {
                return this.versionString == null ? new SemverVersion() : SemverVersion.Parse(this.versionString);
            }
        }

        public IPerson Author { get; }

        public string Description { get; }

        public IKeywords Keywords { get; }

        public IHomepages Homepages { get; }

        public ILicenses Licenses { get; }

        public IFiles Files { get; }

        public IDependencies Dependencies { get; }
        public IDependencies DevDependencies { get; }
        public IBundledDependencies BundledDependencies { get; }
        public IDependencies OptionalDependencies { get; }
        public IDependencies AllDependencies { get; }
        public IEnumerable<string> RequiredBy { get; }
    }
}
