using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RhythmViewExtension
{
    public class RhythmMessageBoxViewModel

    {
    private string _userMessage;

    public string UserMessage
    {
        get { return _userMessage; }
        set => _userMessage = value;
    }

    private bool _wrongVersionLoaded;

    public bool WrongVersionLoaded
    {
        get { return _wrongVersionLoaded; }
        set => _wrongVersionLoaded = value;
    }

    }
}
