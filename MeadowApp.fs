namespace RainSnsr

open System
open Meadow
open Meadow.Devices
open Meadow.Foundation.Leds
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open System.Threading.Tasks

type MeadowApp() =
    inherit App<F7FeatherV1>()

    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable lightRelay : Relay = null
    let mutable rainSensor : IDigitalInputPort = null
    let mutable port0 : IPwmPort = null
    let mutable wiperServo : Servo = null

    let mutable led : RgbPwmLed = null
    
    let retractAwning =
        async {
            Resolver.Log.Info("Setting Retract Relay...")
            retractRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            retractRelay.Toggle()
        }
            
    let expandAwning =
        async {
            Resolver.Log.Info("Setting Extend Relay...")
            extendRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            extendRelay.Toggle()
        }
            
    let stopAwning =
        async {
            Resolver.Log.Info("Setting Stop Relay...")
            stopRelay.Toggle()
            do! Task.Delay(150) |> Async.AwaitTask
            stopRelay.Toggle()
        }

    let ShowColor (color : Color) (duration : TimeSpan) : Task = task {
        led.SetColor(color, 15.0f) 
        do! Async.Sleep duration
    }

    let StopLED () = task {
        led.SetBrightness(0.0f) 
        do! Async.Sleep(TimeSpan.FromMilliseconds(1000.0))
    }

    let CycleColors (duration : TimeSpan) = task {
        do Resolver.Log.Info "Cycle colors..."

        while true do
            do! ShowColor Color.Red duration 
            do! StopLED()
            do! ShowColor Color.Blue duration
            do! StopLED()
            do! ShowColor Color.Green duration
            do! StopLED()
            do! ShowColor Color.Aqua duration 
            do! StopLED()
            do! ShowColor Color.Gold duration
            do! StopLED()
            do! ShowColor Color.Magenta duration
            do! StopLED()
            do! ShowColor Color.White duration
            do! StopLED()
    }

    override this.Initialize() =
        Console.WriteLine("Creating Outputs")
        retractRelay <- Relay(MeadowApp.Device.Pins.C3)
        stopRelay <- Relay(MeadowApp.Device.Pins.C4)
        extendRelay <- Relay(MeadowApp.Device.Pins.C5)
        lightRelay <- Relay(MeadowApp.Device.Pins.C7)
        rainSensor <- MeadowApp.Device.Pins.C6.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        
        do Resolver.Log.Info "Initialize..."

        led <- new RgbPwmLed(
            MeadowApp.Device.Pins.D02,
            MeadowApp.Device.Pins.D03, 
            MeadowApp.Device.Pins.D04)

        Task.CompletedTask;

    override this.Run () =
        task {
            do Resolver.Log.Info "Run..."
            do! CycleColors(TimeSpan.FromSeconds(1.0))
        }