using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinGuiPackaged.model;

namespace WinGuiPackaged {
    public interface ICdViewModel {

        public Cd SelectedCd { get; set; }
        public ObservableCollection<Cd> Cds { get; }
    
    }
}
