﻿using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface IDocumentServices
    {
        IEnumerable<DocumentModals> GetAllData();
    }
}
