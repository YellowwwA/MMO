﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountServer.DB;

public static class Extensions
{
    public static bool SaveChangesEx(this AppDbContext db)
    {
        try
        {
            db.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
