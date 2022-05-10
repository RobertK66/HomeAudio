using Sharpcaster.Interfaces;

namespace QueueCaster {
    
    public class ConsoleWrapper : IConsoleWrapper {
        Action<string> Writeline1 = (line) => Console.WriteLine("QueueCaster: " + line);
        Action<string, Exception, object> Writeline2 = (line, ex, p) => { };

        public ConsoleWrapper(Action<string>? wl1 = null, Action<string, Exception, object>? wl2 = null) {
            Writeline1 = wl1 ?? ((line) => Console.WriteLine("QueueCaster: " + line)); 
            Writeline2 = wl2 ?? ((line, ex, p) => { });   
        }

        

        public void WriteLine(string line) {
            Writeline1(line);
        }

        public void WriteLine(string line, Exception ex, object p) {
            Writeline2(line, ex, p);    
        }
    }
    
}
