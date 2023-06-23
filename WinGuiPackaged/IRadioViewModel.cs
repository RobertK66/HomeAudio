using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGuiPackaged.model;

namespace WinGuiPackaged {
    public interface IRadioViewModel {
        public ILoggerFactory LoggerFactory { get; set; }

        public NamedUrl SelectedRadio { get; set; }
        public ObservableCollection<NamedUrl> WebRadios { get; }
    
    }
}
