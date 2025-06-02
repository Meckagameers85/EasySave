using EasySaveProject.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasySaveProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebAPIController : ControllerBase
    {
        [HttpGet("backups")]
        public ActionResult<List<SaveTask>> GetBackups()
        {
            try
            {
                var backups = BackupManager.instance.GetBackups();
                return Ok(backups);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de la récupération des backups: {ex.Message}");
            }
        }

        [HttpPost("backups")]
        public ActionResult AddBackup([FromBody] SaveTask saveTask)
        {
            try
            {
                if (saveTask == null)
                {
                    return BadRequest("Le SaveTask ne peut pas être null");
                }

                BackupManager.instance.AddBackup(saveTask);
                return Ok(new { message = "Backup ajouté avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de l'ajout du backup: {ex.Message}");
            }
        }

        [HttpDelete("backups")]
        public ActionResult ClearBackups()
        {
            try
            {
                BackupManager.instance.ClearBackups();
                return Ok(new { message = "Tous les backups ont été supprimés" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de la suppression des backups: {ex.Message}");
            }
        }

        // [HttpGet("backups/{id}")]
        // public ActionResult<SaveTask> GetBackup(int id)
        // {
        //     try
        //     {
        //         var backup = BackupManager.instance.GetBackups().FirstOrDefault(b => b.Id == id);
        //         if (backup == null)
        //         {
        //             return NotFound($"Backup avec l'ID {id} non trouvé");
        //         }
        //         return Ok(backup);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest($"Erreur lors de la récupération du backup: {ex.Message}");
        //     }
        // }

        // [HttpDelete("backups/{id}")]
        // public ActionResult DeleteBackup(int id)
        // {
        //     try
        //     {
        //         // Supposons qu'il y ait une méthode DeleteBackup dans BackupManager
        //         bool success = BackupManager.instance.DeleteBackup(id);
        //         if (!success)
        //         {
        //             return NotFound($"Backup avec l'ID {id} non trouvé");
        //         }
        //         return Ok(new { message = $"Backup {id} supprimé avec succès" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest($"Erreur lors de la suppression du backup: {ex.Message}");
        //     }
        // }
    }
}