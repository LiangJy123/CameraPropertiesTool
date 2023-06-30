#pragma once
#include "PhotoModePropertyPayload.g.h"
#include "KSHelper.h"

namespace winrt::CameraKsPropertyHelper::implementation
{
    struct PhotoModePropertyPayload :
        PhotoModePropertyPayloadT<PhotoModePropertyPayload>,
        PropertyValuePayloadHolder<KsBasicCameraExtendedPropPayload>
    {
        PhotoModePropertyPayload(Windows::Foundation::IPropertyValue property)
            : PropertyValuePayloadHolder(property)
        {
            auto tmp_property = reinterpret_cast<KSCAMERA_EXTENDEDPROP_PHOTOMODE*>(&m_propContainer[0] + sizeof(KSCAMERA_EXTENDEDPROP_HEADER));
            m_RequestedHistoryFrames = tmp_property->RequestedHistoryFrames;
            m_MaxHistoryFrames = tmp_property->MaxHistoryFrames;
            m_SubMode = tmp_property->SubMode;

        }



        CameraKsPropertyHelper::ExtendedControlKind ExtendedControlKind() { return ExtendedControlKind::KSPROPERTY_CAMERACONTROL_EXTENDED_PHOTOMODE; }
        int64_t RequestedHistoryFrames() { return static_cast<int64_t>(m_RequestedHistoryFrames); };
        int64_t MaxHistoryFrames() { return static_cast<int64_t>(m_MaxHistoryFrames); };
        int64_t SubMode() { return static_cast<int64_t>(m_SubMode); };

    private:

        int64_t m_RequestedHistoryFrames;
        int64_t m_MaxHistoryFrames;
        int64_t m_SubMode;
    };
}
