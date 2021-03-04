using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Shipbot.Tests.Utils
{
    public abstract class BaseUnitTestClass
    {
        interface IMockWrapper
        {
            void Verify();
        }
        class MockWrapper<T> : IMockWrapper 
            where T : class
        {
            private readonly Mock<T> _mock;
            private readonly Action<Mock<T>>? _verificationFunction;

            public MockWrapper(Mock<T> mock, Action<Mock<T>>? verificationFunction)
            {
                _mock = mock;
                _verificationFunction = verificationFunction;
            }

            public void Verify()
            {
                _verificationFunction?.Invoke(_mock);
                _mock.Verify();
            }
        }
        
        private readonly LoggerFactory _loggerFactory;
        private readonly List<IMockWrapper> _mocks = new List<IMockWrapper>();
        private MockBehavior _defaultMockBehaviour = MockBehavior.Default;

        protected BaseUnitTestClass(ITestOutputHelper testOutputHelper)
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
        }

        protected ILogger<T> GetLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
        
        protected ILogger GetLogger(string? category = null)
        {
            return _loggerFactory.CreateLogger(category ?? "Unnamed");
        }

        protected void UseStrictMocks()
        {
            _defaultMockBehaviour = MockBehavior.Strict;
        }

        protected T MockOf<T>(Action<Mock<T>>? config = null, Action<Mock<T>>? verification = null) where T : class
        {
            var mock = new Mock<T>(_defaultMockBehaviour);

            var configFunction = config ?? (_ => { });
            configFunction(mock);

            _mocks.Add( new MockWrapper<T>(mock, verification));
            
            return mock.Object;
        }

        protected void VerifyMocks()
        {
            foreach (var mock in _mocks)
            {
                mock.Verify();
            }
        }
    }
}