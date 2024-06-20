namespace RainSnsr

open System
open Meadow
open Meadow.Devices
open Meadow.Foundation.Leds
open Meadow.Foundation.Relays
open Meadow.Foundation.Servos
open Meadow.Hardware
open System.Threading.Tasks
open Meadow.Units

type MeadowApp() =
    inherit App<F7FeatherV1>()

    let mutable retractRelay : Relay = null
    let mutable stopRelay : Relay = null
    let mutable extendRelay : Relay = null
    let mutable lightRelay : Relay = null
    let mutable p2relay : Relay = null
    let mutable rainSensor : IDigitalInputPort = null
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

    let StopLED = task {
        led.SetBrightness(0.0f) 
        do! Async.Sleep(TimeSpan.FromMilliseconds(1000.0))
    }

    let CycleColors (duration : TimeSpan) = task {
        do Resolver.Log.Info "Cycle colors..."

        while true do
            do! ShowColor Color.Red duration 
            do! StopLED
            do! ShowColor Color.Blue duration
            do! StopLED
            do! ShowColor Color.Green duration
            do! StopLED
            do! ShowColor Color.Aqua duration 
            do! StopLED
            do! ShowColor Color.Gold duration
            do! StopLED
            do! ShowColor Color.Magenta duration
            do! StopLED
            do! ShowColor Color.White duration
            do! StopLED
    }
    let runServoAsync = task {
        do! wiperServo.RotateTo(Angle(0.0, Angle.UnitType.Degrees)) |> Async.AwaitTask

        while true do
            for i in 0 .. int wiperServo.Config.MaximumAngle.Degrees do
                do! wiperServo.RotateTo(Angle(float i, Angle.UnitType.Degrees)) |> Async.AwaitTask
                Resolver.Log.Info(sprintf "Rotating to %d" i)

            do! Task.Delay(1000) |> Async.AwaitTask

            for i in [180 .. -1 .. int wiperServo.Config.MinimumAngle.Degrees] do
                do! wiperServo.RotateTo(Angle(float i, Angle.UnitType.Degrees)) |> Async.AwaitTask
                Resolver.Log.Info(sprintf "Rotating to %d" i)

            do! Task.Delay(1000) |> Async.AwaitTask
    }

    override this.Initialize() =
        Console.WriteLine("Creating Outputs")
        let servoConfig = ServoConfig(
            maximumAngle = Angle(90, Angle.UnitType.Degrees),
            minimumPulseDuration = 650,
            maximumPulseDuration = 1300
        )
        retractRelay <- Relay(MeadowApp.Device.Pins.D09)
        stopRelay <- Relay(MeadowApp.Device.Pins.D08)
        extendRelay <- Relay(MeadowApp.Device.Pins.D07)
        lightRelay <- Relay(MeadowApp.Device.Pins.D06)
        p2relay <- Relay(MeadowApp.Device.Pins.D05)
        rainSensor <- MeadowApp.Device.Pins.D13.CreateDigitalInputPort(ResistorMode.ExternalPullDown)
        wiperServo <- Servo(MeadowApp.Device.Pins.D12, servoConfig)
        
        do Resolver.Log.Info "Initialize..."

        led <- new RgbPwmLed(
            MeadowApp.Device.Pins.D02,
            MeadowApp.Device.Pins.D03, 
            MeadowApp.Device.Pins.D04)

        Task.CompletedTask;

    override this.Run () =
        task {
            do Resolver.Log.Info "Run LED..."
            do! CycleColors(TimeSpan.FromSeconds(1.0))
        } |> ignore
        task {
            do Resolver.Log.Info "Run Servo..."
            do! runServoAsync
        }