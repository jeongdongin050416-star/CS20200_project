namespace CliFortress

module Program =
    [<EntryPoint>]
    let main args =
        try
            let world = Game.createWorld Game.defaultConfig
            Game.gameLoop Game.defaultConfig world User1
            0
        with ex ->
            eprintfn "Error: %s" ex.Message
            1
