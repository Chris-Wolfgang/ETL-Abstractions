using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfgang.Etl.Abstractions
{
    internal class Class1
    {
        public void Update(string author)
        {
            // Testing CodeQL
            var commandText = "SELECT * FROM books WHERE author LIKE '%" + author + "%'";
        }
    }
}
