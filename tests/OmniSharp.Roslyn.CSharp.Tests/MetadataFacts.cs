using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.Models;
using OmniSharp.Roslyn.CSharp.Services.Navigation;
using OmniSharp.Services;
using TestUtility;
using TestUtility.Annotate;
using Xunit;

namespace OmniSharp.Roslyn.CSharp.Tests
{
    public class MetadataFacts
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IOmnisharpAssemblyLoader _loader;

        public MetadataFacts()
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddConsole();
            _logger = _loggerFactory.CreateLogger<GoToDefinitionFacts>();

            _loader = new AnnotateAssemblyLoader(_logger);
        }

        [Fact]
        public async Task ReturnsSource_ForSpecialType()
        {
            var source1 = @"using System;

class Foo {
}";
            var source2 = @"class Bar {
    private Foo foo;
}";

            var workspace = await TestHelpers.CreateWorkspace(new []
            {
                new TestFile("foo.cs", source1),
                new TestFile("bar.cs", source2)
            });

            var controller = new MetadataService(workspace, new MetadataHelper(_loader));
            var response = await controller.Handle(new MetadataRequest
            {
                AssemblyName = "System.Private.CoreLib",
                TypeName = "System.String",
                Timeout = 60000
            });

            Assert.NotNull(response.Source);
        }

        [Fact]
        public async Task ReturnsSource_ForNormalType()
        {
            var source1 = @"using System;

class Foo {
}";
            var source2 = @"class Bar {
    private Foo foo;
}";

            var workspace = await TestHelpers.CreateWorkspace(new []
            {
                new TestFile("foo.cs", source1),
                new TestFile("bar.cs", source2)
            });

            var controller = new MetadataService(workspace, new MetadataHelper(_loader));
            var response = await controller.Handle(new MetadataRequest
            {
#if NETCOREAPP1_0
                AssemblyName = "System.Linq",
#else
                AssemblyName = "System.Core",
#endif
                TypeName = "System.Linq.Enumerable",
                Timeout = 60000
            });

            Assert.NotNull(response.Source);
        }

        [Fact]
        public async Task ReturnsSource_ForGenericType()
        {
            var source1 = @"using System;

class Foo {
}";
            var source2 = @"class Bar {
    private Foo foo;
}";

            var workspace = await TestHelpers.CreateWorkspace(new []
                {
                    new TestFile("foo.cs", source1),
                    new TestFile("bar.cs", source2)
                });

            var controller = new MetadataService(workspace, new MetadataHelper(_loader));
            var response = await controller.Handle(new MetadataRequest
            {
                AssemblyName = "System.Private.CoreLib",
                TypeName = "System.Collections.Generic.List`1",
                Timeout = 60000
            });

            Assert.NotNull(response.Source);

            response = await controller.Handle(new MetadataRequest
            {
                AssemblyName = "System.Private.CoreLib",
                TypeName = "System.Collections.Generic.Dictionary`2"
            });

            Assert.NotNull(response.Source);
        }
    }
}
