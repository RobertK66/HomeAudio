using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace TestBasis {
    public class TestHelper {
        private ITestOutputHelper output;
        public List<string>? AssertableTestLog = null;

        public TestHelper(ITestOutputHelper outp) {
            output = outp;
        }

        private Mock<ILogger<T>> CreateILoggerMock<T>() {
            Mock<ILogger<T>> retVal = new Mock<ILogger<T>>();
            retVal.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation => {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });

                    var testingName = typeof(T).GetGenericArguments().FirstOrDefault()?.Name;

                    try {
                        output?.WriteLine(DateTime.Now.ToLongTimeString() + " " + typeof(T).GetGenericArguments().FirstOrDefault()?.Name + " " + logLevel + " " + logMessage);
                    } catch { }
                    AssertableTestLog?.Add(logMessage);
                }));

            return retVal;
        }


        public ILoggerFactory CreateMockedLoggerFactory(List<string> assertableLog = null) {
            AssertableTestLog = assertableLog;

            var loggerGeneric = new Mock<ILogger>();
            loggerGeneric.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation => {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });

                    try {
                        output?.WriteLine(DateTime.Now.ToLongTimeString() + " GenericMocked " + logLevel + " " + logMessage);
                    } catch { }
                    AssertableTestLog?.Add(logMessage);
                }));

            //var loggerCC = CreateILoggerMock<ILogger<ChromecastClient>>();
            //var loggerHBC = CreateILoggerMock<ILogger<HeartbeatChannel>>();
            //var loggerCCC = CreateILoggerMock<ILogger<ChromecastChannel>>();


            var loggerFactory = new Mock<ILoggerFactory>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns
                (new InvocationFunc(new Func<IInvocation, ILogger>((inv) => {
                    var name = (string)inv.Arguments[0];
                    //if (name == "Sharpcaster.ChromecastClient") {
                    //    return CreateILoggerMock<ILogger<ChromecastClient>>().Object;
                    //} else if (name == "Sharpcaster.Channels.HeartbeatChannel") {
                    //    return CreateILoggerMock<ILogger<HeartbeatChannel>>().Object;
                    //} else if (name == "Sharpcaster.Channels.ChromecastChannel") {
                    //    return CreateILoggerMock<ILogger<ChromecastChannel>>().Object;
                    //} else if (name == "Sharpcaster.Channels.ConnectionChannel") {
                    //    return CreateILoggerMock<ILogger<ConnectionChannel>>().Object;
                    //} else if (name == "Sharpcaster.Channels.MediaChannel") {
                    //    return CreateILoggerMock<ILogger<MediaChannel>>().Object;
                    //} else if (name == "Sharpcaster.Channels.ReceiverChannel") {
                    //    return CreateILoggerMock<ILogger<ReceiverChannel>>().Object;
                    //} else if (name == "Sharpcaster.Channels.MultiZoneChannel") {
                    //    return CreateILoggerMock<ILogger<MultiZoneChannel>>().Object;
                    //} else {
                        return loggerGeneric.Object;
                    //}
                }
            )));

            return loggerFactory.Object;
        }



    }
}
