namespace CliFortress

open System
open System.Threading

type Player = User1 | User2

type Tank = {
    Player: Player
    X: int
    Y: int
    Symbol: char
}

type Shot = {
    Speed: float
    AngleDegrees: float
}

type GameConfig = {
    Width: int
    SkyHeight: int
    MinBuildingHeight: int
    MaxBuildingHeight: int
    MinSpeed: float
    MaxSpeed: float
    MinAngle: float
    MaxAngle: float
    Gravity: float
    TimeStep: float
    FrameDelayMs: int
    Animate: bool
    Quiet: bool
    Seed: int option
}

type World = {
    User1: Tank
    User2: Tank
    Building1Height: int
    Building2Height: int
}

type ShotResult = Hit of Player | Miss of string

module Game =
    let defaultConfig = {
        Width = 80
        SkyHeight = 24
        MinBuildingHeight = 3
        MaxBuildingHeight = 10
        MinSpeed = 1.0
        MaxSpeed = 100.0
        MinAngle = 0.0
        MaxAngle = 90.0
        Gravity = 9.8
        TimeStep = 0.10
        FrameDelayMs = 45
        Animate = true
        Quiet = false
        Seed = None
    }

    let otherPlayer = function
        | User1 -> User2
        | User2 -> User1

    let playerName = function
        | User1 -> "User1"
        | User2 -> "User2"

    let createWorld (config: GameConfig) =
        let random =
            match config.Seed with
            | Some seed -> Random(seed)
            | None -> Random()

        let h1 = random.Next(config.MinBuildingHeight, config.MaxBuildingHeight + 1)
        let h2 = random.Next(config.MinBuildingHeight, config.MaxBuildingHeight + 1)
        let x1 = 10
        let x2 = config.Width - 11
        {
            Building1Height = h1
            Building2Height = h2
            User1 = { Player = User1; X = x1; Y = h1; Symbol = '1' }
            User2 = { Player = User2; X = x2; Y = h2; Symbol = '2' }
        }

    let render (config: GameConfig) (world: World) (ball: (int * int) option) (status: string) =
        if not config.Quiet then
            if config.Animate then Console.Clear()
            printfn "%s" status
            printfn "Speed range: %.0f..%.0f | Angle range: %.0f..%.0f degrees" config.MinSpeed config.MaxSpeed config.MinAngle config.MaxAngle
            printfn "Coordinates: User1=(%d,%d), User2=(%d,%d)" world.User1.X world.User1.Y world.User2.X world.User2.Y
            printfn ""

            for y in [config.SkyHeight .. -1 .. 0] do
                let chars = Array.create config.Width ' '
                for x in 0 .. config.Width - 1 do
                    if y = 0 then chars[x] <- '='
                    elif x >= world.User1.X - 2 && x <= world.User1.X + 2 && y <= world.Building1Height then chars[x] <- '#'
                    elif x >= world.User2.X - 2 && x <= world.User2.X + 2 && y <= world.Building2Height then chars[x] <- '#'

                if world.User1.Y = y then chars[world.User1.X] <- world.User1.Symbol
                if world.User2.Y = y then chars[world.User2.X] <- world.User2.Symbol

                match ball with
                | Some (bx, by) when by = y && bx >= 0 && bx < config.Width -> chars[bx] <- '*'
                | _ -> ()

                new String(chars) |> printfn "%s"

    let private readFloatInRange prompt minValue maxValue =
        let rec loop () =
            printf "%s" prompt
            let input = Console.ReadLine()
            match Double.TryParse(input) with
            | true, value when value >= minValue && value <= maxValue -> value
            | true, _ ->
                printfn "Invalid value. Enter a number from %.0f to %.0f." minValue maxValue
                loop ()
            | false, _ ->
                printfn "Invalid input. Enter a numeric value."
                loop ()
        loop ()

    let readShot (config: GameConfig) player =
        printfn ""
        printfn "%s's turn" (playerName player)
        let speed = readFloatInRange "Speed: " config.MinSpeed config.MaxSpeed
        let angle = readFloatInRange "Angle: " config.MinAngle config.MaxAngle
        { Speed = speed; AngleDegrees = angle }

    let private roundedPosition (x: float) (y: float) =
        int (Math.Round(x, MidpointRounding.AwayFromZero)),
        int (Math.Round(y, MidpointRounding.AwayFromZero))

    let simulateShot (config: GameConfig) (world: World) shooter shot =
        let shooterTank, targetTank, direction =
            match shooter with
            | User1 -> world.User1, world.User2, 1.0
            | User2 -> world.User2, world.User1, -1.0

        let radians = shot.AngleDegrees * Math.PI / 180.0
        let vx = direction * shot.Speed * Math.Cos(radians)
        let vy = shot.Speed * Math.Sin(radians)

        let mutable result: ShotResult option = None
        let mutable previousCell: (int * int) option = None
        let mutable t = config.TimeStep

        while result.IsNone do
            let x = float shooterTank.X + vx * t
            let y = float shooterTank.Y + vy * t - 0.5 * config.Gravity * t * t
            let bx, by = roundedPosition x y

            if previousCell <> Some (bx, by) then
                previousCell <- Some (bx, by)
                render config world (Some (bx, by)) (sprintf "%s fires: speed=%.1f angle=%.1f" (playerName shooter) shot.Speed shot.AngleDegrees)
                if config.Animate then Thread.Sleep(config.FrameDelayMs)

            if bx = targetTank.X && by = targetTank.Y then
                result <- Some (Hit targetTank.Player)
            elif bx = shooterTank.X && by = shooterTank.Y then
                // Requirement: a user cannot hit their own tank. Ignore the launcher's own cell.
                ()
            elif by <= 0 then
                result <- Some (Miss "The cannonball fell to the ground.")
            elif bx < 0 || bx >= config.Width then
                result <- Some (Miss "The cannonball left the battlefield.")
            elif shooter = User1 && bx > targetTank.X then
                result <- Some (Miss "The cannonball passed behind User2.")
            elif shooter = User2 && bx < targetTank.X then
                result <- Some (Miss "The cannonball passed behind User1.")

            t <- t + config.TimeStep

        result.Value

    let rec gameLoop config world currentPlayer =
        render config world None (sprintf "CLI FORTRESS - %s to attack" (playerName currentPlayer))
        let shot = readShot config currentPlayer
        match simulateShot config world currentPlayer shot with
        | Hit User1 ->
            render config world (Some (world.User1.X, world.User1.Y)) "User1's tank was hit!"
            printfn "User2 wins!"
        | Hit User2 ->
            render config world (Some (world.User2.X, world.User2.Y)) "User2's tank was hit!"
            printfn "User1 wins!"
        | Miss reason ->
            printfn "%s Miss. Attack opportunity passes to %s." reason (otherPlayer currentPlayer |> playerName)
            if config.Animate then Thread.Sleep(900)
            gameLoop config world (otherPlayer currentPlayer)

    let parseArgs (args: string array) =
        let rec loop i config =
            if i >= args.Length then config
            else
                match args[i] with
                | "--no-animation" -> loop (i + 1) { config with Animate = false; FrameDelayMs = 0 }
                | "--seed" when i + 1 < args.Length ->
                    match Int32.TryParse(args[i + 1]) with
                    | true, seed -> loop (i + 2) { config with Seed = Some seed }
                    | false, _ -> failwith "--seed requires an integer."
                | "--help" | "-h" ->
                    printfn "CLI FORTRESS"
                    printfn "Usage: dotnet run [-- --seed N] [--no-animation]"
                    Environment.Exit(0)
                    config
                | unknown -> failwithf "Unknown argument: %s" unknown
        loop 0 defaultConfig


    let runSelfTests () =
        let config = { defaultConfig with Width = 30; SkyHeight = 10; Animate = false; Quiet = true; TimeStep = 0.10 }
        let world = {
            Building1Height = 5
            Building2Height = 5
            User1 = { Player = User1; X = 10; Y = 5; Symbol = '1' }
            User2 = { Player = User2; X = 20; Y = 5; Symbol = '2' }
        }

        let assertResult name expected actual =
            if actual <> expected then
                failwithf "%s failed. Expected %A but got %A." name expected actual
            else
                printfn "PASS: %s" name

        let hitShot = { Speed = Math.Sqrt(98.0); AngleDegrees = 45.0 }
        assertResult "User1 can hit User2 by exact displayed coordinate" (Hit User2) (simulateShot config world User1 hitShot)
        assertResult "User2 can hit User1 by exact displayed coordinate" (Hit User1) (simulateShot config world User2 hitShot)

        match simulateShot config world User1 { Speed = 5.0; AngleDegrees = 90.0 } with
        | Miss _ -> printfn "PASS: missed shot eventually falls and passes turn"
        | result -> failwithf "Vertical miss test failed. Expected Miss but got %A." result

        printfn "All CLI FORTRESS self-tests passed."

module Program =
    [<EntryPoint>]
    let main args =
        try
            if args |> Array.contains "--self-test" then
                Game.runSelfTests ()
                0
            else
                let config = Game.parseArgs args
                let world = Game.createWorld config
                Game.gameLoop config world User1
                0
        with ex ->
            eprintfn "Error: %s" ex.Message
            1
