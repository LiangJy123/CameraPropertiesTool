#pragma once
#include "DigitalWindowConfigCaps.g.h"
#include "KSHelper.h"
namespace winrt::CameraKsPropertyHelper::implementation
{
    struct DigitalWindowConfigCaps : DigitalWindowConfigCapsT<DigitalWindowConfigCaps>
    {
        DigitalWindowConfigCaps(KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPS configCaps)
            : m_configCaps(configCaps) {};

        int64_t ResolutionX() { return static_cast<int64_t>(m_configCaps.ResolutionX); };
        int64_t ResolutionY() { return static_cast<int64_t>(m_configCaps.ResolutionY); };
        int64_t PorchTop() { return static_cast<int64_t>(m_configCaps.PorchTop); };
        int64_t PorchLeft() { return static_cast<int64_t>(m_configCaps.PorchLeft); };
        int64_t PorchBottom() { return static_cast<int64_t>(m_configCaps.PorchBottom); };
        int64_t PorchRight() { return static_cast<int64_t>(m_configCaps.PorchRight); };
        int64_t NonUpscalingWindowSize() { return static_cast<int64_t>(m_configCaps.NonUpscalingWindowSize); };
        int64_t MinWindowSize() { return static_cast<int64_t>(m_configCaps.MinWindowSize); };
        int64_t MaxWindowSize() { return static_cast<int64_t>(m_configCaps.MaxWindowSize); };

    private:
        KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPS m_configCaps;
    };

}
/*
    LONG        ResolutionX;            // Output width in pixels
    LONG        ResolutionY;            // Output height in pixels
    LONG        PorchTop;               // Upper porch region in Q24
    LONG        PorchLeft;              // Left-most porch region in Q24
    LONG        PorchBottom;            // Lower porch region in Q24
    LONG        PorchRight;             // Right-most porch region in Q24
    LONG        NonUpscalingWindowSize; // Q24 value to get no scaling
    LONG        MinWindowSize;          // Smallest legal WindowSize
    LONG        MaxWindowSize;          // Largest legal WindowSize
    LONG        Reserved;               // Reserved
*/