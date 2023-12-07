﻿using System.Collections.Generic;
using System.IO;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Moq;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class DefaultParameterProcessorTests
    {
        private readonly IParameterProcessor _parameterProcessor;

        public DefaultParameterProcessorTests()
        {
            _parameterProcessor = new DefaultParameterProcessor();
        }

        [Fact]
        public void ProcessParametersTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/p1/p2-1", Value = "p1:p2-1"},
                new Parameter {Name = "/start/path/p1/p2-2", Value = "p1:p2-2"},
                new Parameter {Name = "/start/path/p1/p2/p3-1", Value = "p1:p2:p3-1"},
                new Parameter {Name = "/start/path/p1/p2/p3-2", Value = "p1:p2:p3-2"},
                new Parameter {Name = "/start/path/dup/a", Value = "dup:a"},
                new Parameter {Name = "/start/path/DUP/A", Value = "DUP:A"},
            };

            const string path = "/start/path";

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void ProcessParametersStringListTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/string-list/single", Value = "p1", Type = ParameterType.StringList},
                new Parameter {Name = "/string-list/multiple", Value = "p1,p2,p3", Type = ParameterType.StringList},
                new Parameter {Name = "/string-list/empty", Value = "", Type = ParameterType.StringList},
            };

            const string path = "/string-list";

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.Equal(5, data.Keys.Count);
            Assert.Equal("p1", data["single:0"]);
            Assert.Equal("p1", data["multiple:0"]);
            Assert.Equal("p2", data["multiple:1"]);
            Assert.Equal("p3", data["multiple:2"]);
            Assert.Equal("", data["empty:0"]);
        }

        
        [Fact]
        public void ProcessParametersRootTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/p1", Value = "p1"},
                new Parameter {Name = "p2", Value = "p2"},
            };

            const string path = "/";

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(data, item => Assert.Equal(item.Value, item.Key));
        }

        [Fact]
        public void ProcessParametersDuplicateKeyTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/start/path/dup/a", Value = "dup:a"},
                new Parameter {Name = "/start/path/dup/a", Value = "dup:a"},
            };

            const string path = "/start/path";

            Assert.Throws<InvalidDataException>(() => _parameterProcessor.ProcessParameters(parameters, path));
        }
    }
}