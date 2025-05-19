module HandleStart
open System
open System.Diagnostics
open System.IO
open System.Net

let handleStart (folderFlag: bool) =
    
    let currentDirectory = Environment.CurrentDirectory
    printfn "Current Directory: %s" currentDirectory

    let generatedCode = 
        $"""
        MODNAME: insert_here,
        TRAITS
        [
            {{
            ID: insert_here
            HEALTH: 0,
            DAMAGE: 0,
            CRIT_CHANCE: 0,
            RANGE: 0,
            ATTACK_SPEED: 0,
            DODGE: 0,
            ACCURACY: 0,
            SCALE: 0,
            INTELLIGENCE: 0,
            WARFARE: 0,
            PATH: null, //put your path here
            DESC: describe dis,
            POWERS: @wizardry, //put your real powers here
            SPEED: 0
            }}
        ]
        EFFECTS
        [
            {{
            ID: insert_here,
            CHANCE: 0.5,
            SPAWNS_FROM_ACTOR: true,
            SPAWNS_ON_TARGET: false,
            IS_ATTACK: true,
            TIMEBETWEENFRAMES: 0.1,
            SPRITE: path_to_sprite,
            DRAW_LIGHT: true,
            LIGHT_SIZE: 5.0,
            LIMIT: 10,
            COMBINE: @wizardy //put your real combinations here
            }}
        ]
        PROJECTILES
        [
            {{
            ID: insert_here,
            CHANCE: 0.75,
            TEXTURE: path_to_texture,
            SPEED: 10.0,
            DRAW_LIGHT_AREA: true,
            LIGHT_SIZE: 4.0,
            PARABOLIC: true,
            SCALE: 1.0,
            ANIMATION_SPEED: 0.1,
            LOOKING_AT_TARGET: true,
            TERRAFORM_OPTION: null,
            COMBINE: @wizardy // put your real combinations here
            }}
        ]
        TERRAFORMING
        [
            {{
            ID: insert_here,
            ADD_BURNED: true,
            FLASH: true,
            APPLY_FORCE: true,
            FORCE_POWER: 5.0,
            EXPLODE_TILE: true,
            EXPLODE_STRENGTH: 2.0,
            DAMAGEBUILDINGS: true,
            SET_FIRE: true,
            SHAKE: true,
            DAMAGE: 50.0,
            DESC: terraform effect description
            }}
        ]
        """
    if folderFlag then
         let sections = [
            ("TRAITS.wrldbx", "TRAITS");
            ("EFFECTS.wrldbx", "EFFECTS");
            ("PROJECTILES.wrldbx", "PROJECTILES");
            ("TERRAFORMING.wrldbx", "TERRAFORMING")
         ]

         for (fileName, section) in sections do
            let sectionContent = 
                // Extract the part of the generated code that corresponds to the section
                let startIdx = generatedCode.IndexOf(section)
                let endIdx = generatedCode.IndexOf("]", startIdx)
                generatedCode.Substring(startIdx, endIdx - startIdx + 1)

            // Define the full file path
            let filePath = Path.Combine(currentDirectory, fileName)

            // Write the section content to the file
            File.WriteAllText(filePath, sectionContent)
    else
        let filePath = Path.Combine(currentDirectory, "Main.wrldbx")
        File.WriteAllText(currentDirectory, generatedCode)
