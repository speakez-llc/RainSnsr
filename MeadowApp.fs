namespace RainSnsr

open System
open System.Threading.Tasks
open Meadow
open Meadow.Devices
open Meadow.Foundation.Leds
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open Meadow.Units

type MeadowApp() =
    inherit App<F7FeatherV1>()

    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable lightRelay : Relay = null
    let mutable p2relay : Relay = null
    let mutable pwmPort : IPwmPort = null
    let mutable rainSensor : IDigitalInputPort = null
    let mutable wiperServo : AngularServo = null
    let mutable led : RgbPwmLed = null
    
    let toggleRelay (relay : Relay) (message : string) =
        async {
            Resolver.Log.Info(message)
            relay.Toggle()
            do! Async.Sleep 150
            relay.Toggle()
        }
    
    let ShowColor (color : Color) (duration : TimeSpan) = async {
        led.SetColor(color, 15.0f) 
        do! Async.Sleep duration
        led.SetBrightness(0.0f)
    }

    let getRainState : Async<bool> = async {
        if rainSensor.State then
            Resolver.Log.Info("It's raining!")
            return true
        else
            Resolver.Log.Info("No Rain!")
            return false
    }
    
    let clearRainSnsr = async {
        Resolver.Log.Info "Clearing Sensor..."
        let stopwatch = System.Diagnostics.Stopwatch()
        stopwatch.Start()
        while stopwatch.Elapsed.TotalSeconds < 5.0 do
            do! Async.AwaitTask (Task.Run(fun () -> wiperServo.RotateTo(Angle 5)))
            do! Async.AwaitTask (Task.Run(fun () -> wiperServo.RotateTo(Angle 90)))
        Resolver.Log.Info "Sensor Clear Complete
        ..."
        do! Async.Sleep 2000
    }
        
    let runRainSnsrAsync = async {
        do! Async.AwaitTask (Task.Run(fun () -> wiperServo.RotateTo(Angle 0)))
        Resolver.Log.Info "Starting Rain Sensor..."
        while true do
            let rainState = getRainState |> Async.RunSynchronously
            if rainState then
                do! toggleRelay retractRelay "Retracting awning..."
                do! ShowColor Color.Aqua (TimeSpan.FromMilliseconds 500)
                do! Async.AwaitTask (Task.Run(fun () -> wiperServo.RotateTo(Angle 180)))
                do! clearRainSnsr
                do! Async.AwaitTask (Task.Run(fun () -> wiperServo.RotateTo(Angle 0)))
                do! Async.Sleep 2000
            else 
                do! Async.Sleep 15000
    }

    override this.Initialize() =
        Resolver.Log.Info "Initialize..."
        led <- new RgbPwmLed(
            MeadowApp.Device.Pins.D02,
            MeadowApp.Device.Pins.D03, 
            MeadowApp.Device.Pins.D04)
        retractRelay <- Relay(MeadowApp.Device.Pins.D09)
        stopRelay <- Relay(MeadowApp.Device.Pins.D08)
        extendRelay <- Relay(MeadowApp.Device.Pins.D07)
        lightRelay <- Relay(MeadowApp.Device.Pins.D06)
        p2relay <- Relay(MeadowApp.Device.Pins.D05)
        rainSensor <- MeadowApp.Device.Pins.D13.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        pwmPort <- MeadowApp.Device.CreatePwmPort(MeadowApp.Device.Pins.D12, Frequency 50)
        wiperServo <- new AngularServo(pwmPort, AngularServo.PulseAngle(Angle 0, TimeSpan.FromMilliseconds 1.0),
                                       AngularServo.PulseAngle(Angle 180, TimeSpan.FromMilliseconds 2.4))
        
        Task.CompletedTask
    override this.Run () = task {
        do! runRainSnsrAsync
       }