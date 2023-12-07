﻿using System.Collections.Generic;
using System.IO;
using Amazon.SimpleSystemsManagement.Model;
using Xunit;

namespace Amazon.Extensions.Configuration.SystemsManager.Tests
{
    public class JsonParameterProcessorTests
    {
        private readonly IParameterProcessor _parameterProcessor;

        public JsonParameterProcessorTests()
        {
            _parameterProcessor = new JsonParameterProcessor();
        }

        [Fact]
        public void ProcessParametersTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/p1", Value = "{\"p1\": \"p1\"}"},
                new Parameter {Name = "p2", Value = "{\"p2\": \"p2\"}"},
                new Parameter {Name = "/p1/p3", Value = "{\"p3key\": \"p3value\"}"},
                new Parameter {Name = "/p4", Value = "{\"p4key\": { \"p5key\": \"p5value\" } }"},
                new Parameter {Name = "/p6", Value = "{\"p6key\": { \"p7key\": { \"p8key\": \"p8value\" } } }"},
                new Parameter {Name = "/ObjectA", Value = "{\"Bucket\": \"arnA\"}"},
                new Parameter {Name = "/ObjectB", Value = "{\"Bucket\": \"arnB\"}"},
                new Parameter {Name = "/", Value = "{\"testParam\": \"testValue\"}"}
            };
            var expected = new Dictionary<string, string>() {
                { "p1:p1", "p1" },
                { "p2:p2", "p2" },
                { "p1:p3:p3key", "p3value" },
                { "p4:p4key:p5key", "p5value" },
                { "p6:p6key:p7key:p8key", "p8value" },
                { "ObjectA:Bucket", "arnA" },
                { "ObjectB:Bucket", "arnB" },
                { "testParam", "testValue" }
            };

            const string path = "/";

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(expected, item => Assert.Equal(item.Value, data[item.Key]));
        }

        [Fact]
        public void ProcessParametersMergeTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/dup", Value = "{\"testParam1\": \"testValue\"}"},
                new Parameter {Name = "/DUP", Value = "{\"testParam2\": \"testValue\"}"},
            };
            var expected = new Dictionary<string, string>() {
                { "dup:testParam1", "testValue" },
                { "DUP:testParam2", "testValue" },
            };

            const string path = "/";

            var data = _parameterProcessor.ProcessParameters(parameters, path);

            Assert.All(expected, item => Assert.Equal(item.Value, data[item.Key]));
        }

        [Fact]
        public void ProcessParametersDuplicateKeyTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/dup", Value = "{\"testParam\": \"testValue\"}"},
                new Parameter {Name = "/DUP", Value = "{\"testParam\": \"testValue\"}"},
            };
            const string path = "/";

            Assert.Throws<InvalidDataException>(() => _parameterProcessor.ProcessParameters(parameters, path));
        }

        [Fact]
        public void ProcessParametersDuplicateJsonKeyTest()
        {
            var parameters = new List<Parameter>
            {
                new Parameter {Name = "/dup", Value = "{\"testParam\": \"testValue1\", \"TestParam\": \"testValue2\"}"},
            };
            const string path = "/";

            Assert.Throws<InvalidDataException>(() => _parameterProcessor.ProcessParameters(parameters, path));
        }
    }
}