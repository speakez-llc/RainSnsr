# Logic

The sensor will have a variety of functions aside from the core case of detecting rainfall and retracting the awning. This page is to track all of the ancilliary operations and secondary behaviors such as interacting the the OpenWeather API and receiving commands from Meadow Cloud to check the state of the device.

- [ ] Basic Commands
  - [ ] Retract Awning
  - [ ] Stop Awning
  - [ ] Extend Awning
  - [ ] Toggle Awning Light
  - [ ] Get Servo Position
- [ ] Compound Commands
  - [ ] P2 double tap with retract double tap
  - [ ] P2 single tap with retract single tap
  - [ ] Double Toggle Awning Light
  - [ ] Retract awning after sundown
  - [ ] Extend awning after sunrise
- [ ] State Elements
  - [ ] Awning Position
  - [ ] Weather Conditions
  - [ ] Interval Since Weather Cleared
  - [ ] Set Weather Check Interval
  - [ ] Last Weather Check
  - [ ] Last Weather Alert
  - [ ] Last Rain Reported
  - [ ] Last Wind Reported
  - [ ] Last Rain Event
  - [ ] Last Retract
  - [ ] Cycle Count
  - [ ] Last Extend
  - [ ] Last Device State Check
  - [ ] Location

- [ ] Check Weather every 10/15 minutes
  - [ ] If rain is due, partially retract the awning
  - [ ] If wind is due, partially retract the awning

- [ ] Check Device State
  - [ ] Get Servo Angle
  - [ ] Get Meadow Stats