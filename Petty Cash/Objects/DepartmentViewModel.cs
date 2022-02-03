using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Objects
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string JobDescription { get; set; }

        public DepartmentViewModel()
        { }

        public DepartmentViewModel(Petty_Cash.Model.department entity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.DepartmentName = entity.DepartmentName;
            this.JobDescription = entity.JobDescription;
        }
    }
}
