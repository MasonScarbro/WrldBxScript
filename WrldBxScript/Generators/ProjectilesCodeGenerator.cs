using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public  class ProjectilesCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public ProjectilesCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");

            // Add effects-specific generation logic here
            foreach (WrldBxProjectile projectile in _repositories["PROJECTILES"].GetAll.Cast<WrldBxProjectile>())
            {
                AddBlockId(src, projectile.id);
                if (projectile.texture.Equals("fireball"))
                {
                    WrldBxScript.Warning($"{projectile.id} Does not have an assigned texture, given default texture");
                }
                if (!_repositories["TERRAFORMING"].Exists(projectile.terraformOption))
                {
                    //When we introduce Globals we may need to change this
                    WrldBxScript.Warning($"{projectile.terraformOption} Does not exist within TERRAFORMING");
                    projectile.terraformOption = string.Empty;
                }
                src.Append($"\t\t\ndraw_light_area = {projectile.draw_light_area}," +
                       $"\t\t\ndraw_light_size = {projectile.draw_light_size}," +
                       $"\t\t\ntexture = {InQuotes(projectile.texture)},");
                src.Append(projectile.animation_speed.HasValue ? $"\t\t\nanimation_speed = {projectile.animation_speed}" : "");
                src.Append($"\t\t\nspeed = {projectile.speed}," +
                       $"\t\t\nparabolic = {StringHelpers.ConvertBoolString(projectile.parabolic)}" +
                       $"\t\t\nlook_at_target = {projectile.lookAtTarget}" +
                       $"\t\t\nstartScale = {projectile.scale}," +
                       $"\t\t\ntargetScale = {projectile.scale}," +
                       $"\t\t\nterraformOption = {projectile.terraformOption},");
                src.Append("\t\t\nlooped = true," +
                       "\t\t\nendEffect = string.Empty," +
                       $"\t\t\ntexture_shadow = {InQuotes("shadow_ball")}," +
                       $"\t\t\ntrailEffect_enabled = true," +
                       $"sound_launch = {InQuotes("event:/SFX/WEAPONS/WeaponFireballStart")},");


                AddReqCodeToBlock(src, projectile.id);
            }

            src.AppendLine("\t}\n}");
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.AppendLine($"\t\tvar {name} = AssetManager.effects_library.add(new EffectAsset {{");
            src.AppendLine($"\t\t\tid = {InQuotes(name.ToString())}");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.AppendLine("\t\t});");
            src.AppendLine($"World.world.stackEffects.CallMethod(add, {InQuotes(name.ToString())});");
        }

        private string InQuotes(string str) => $"\"{str}\"";
    }
}
