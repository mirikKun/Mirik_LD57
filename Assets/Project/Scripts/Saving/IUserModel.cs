using System;

namespace Project.Scripts.Saving
{
    public interface IUserModel
    {
        event Action<string> OnChanged;
    }
}