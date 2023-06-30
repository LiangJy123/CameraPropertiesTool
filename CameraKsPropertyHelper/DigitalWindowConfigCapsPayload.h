#pragma once
#include "DigitalWindowConfigCapsPayload.g.h"
#include "DigitalWindowConfigCaps.h"


namespace winrt::CameraKsPropertyHelper::implementation
{
    struct DigitalWindowConfigCapsPayload :
        DigitalWindowConfigCapsPayloadT<DigitalWindowConfigCapsPayload>,
        PropertyValuePayloadHolder<KsBasicCameraExtendedPropPayload>
    {
        DigitalWindowConfigCapsPayload(Windows::Foundation::IPropertyValue property)
            : PropertyValuePayloadHolder(property)
        {
            int configCapsCount = (m_propContainer.size() - sizeof(KSCAMERA_EXTENDEDPROP_HEADER) - sizeof(KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPSHEADER)) / sizeof(KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPS);

            for (int i = 0; i < configCapsCount; i++)
            {
                auto nativeConfigCaps = reinterpret_cast<KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPS*>(&m_propContainer[0] + sizeof(KSCAMERA_EXTENDEDPROP_HEADER)+ sizeof(KSCAMERA_EXTENDEDPROP_DIGITALWINDOW_CONFIGCAPSHEADER) + i * sizeof(KSCAMERA_EXTENDEDPROP_BACKGROUNDSEGMENTATION_CONFIGCAPS));
                auto runtimeConfigCaps = make<DigitalWindowConfigCaps>(*nativeConfigCaps);
                m_configCaps.Append(runtimeConfigCaps);
            }
        }

        Windows::Foundation::Collections::IVectorView<CameraKsPropertyHelper::DigitalWindowConfigCaps> ConfigCaps() { return m_configCaps.GetView(); }

        CameraKsPropertyHelper::ExtendedControlKind ExtendedControlKind() { return ExtendedControlKind::KSPROPERTY_CAMERACONTROL_EXTENDED_DIGITALWINDOW_CONFIGCAPS; }

    private:
        Windows::Foundation::Collections::IVector<CameraKsPropertyHelper::DigitalWindowConfigCaps> m_configCaps = single_threaded_vector<CameraKsPropertyHelper::DigitalWindowConfigCaps>();
        
    };
}
