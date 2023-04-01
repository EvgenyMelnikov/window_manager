using System;
using UnityEngine;

public interface IViewBlocker
{
    event Action Click;
    
    Transform Transform { get; }

    void FadeIn();

    void FadeOut();
}
