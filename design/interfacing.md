# Interfacing

Aside from the device "reporting out" the status of the conditions and state of the device, there also needs to be a way to send in requests for info and issue commands to both directly control the device and set its internal state in order to make runtime adjustments. 

## Near Term

While this will eventually run completely on SpeakEZ "backplane" infrastructure it will initially be deployed using [Meadow Cloud](https://meadowcloud.co/), which runs on Azure infrastructure. WildernessLabs provides this for free for a small number of devices, which is perfect for the first phase this project.

### WiFi

The Meadow "Feather" has an IPEX mini connector to attach [an external antenna](https://a.co/d/0995EHsz). This will be used to connect to the local WiFi network and then to the Meadow Cloud service.

### Bluetooth Support

The Meadow "Feather" also has a Bluetooth radio. This will be used to connect to the SpeakEZ app on a phone or tablet. This will allow for direct control of the device and for setting the device state. I have done some work in this area for a solar panel project but a great deal of nuanced work remains to be done in order to make it a first-class experience.