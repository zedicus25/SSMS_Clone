﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MSSM_Clone.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
