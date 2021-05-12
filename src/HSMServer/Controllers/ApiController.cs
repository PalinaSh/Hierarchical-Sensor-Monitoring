﻿using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using HSMCommon.Model;
using HSMCommon.Model.SensorsData;
using HSMServer.Authentication;
using HSMServer.MonitoringServerCore;
using NLog;

namespace HSMServer.Controllers
{
    [Route("api/site")]
    [ApiController]
    internal class ApiController : ControllerBase
    {
        private readonly Logger _logger;
        private readonly IMonitoringCore _monitoringCore;
        public ApiController(IMonitoringCore monitoringCore)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _monitoringCore = monitoringCore;
        }

        [HttpGet("GetTree")]
        public ActionResult<List<SensorData>> GetTree()
        {
            try
            {
                return _monitoringCore.GetSensorsTree(HttpContext.User as User);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return BadRequest();
            }
        }

        [HttpGet("GetUpdates")]
        public ActionResult<List<SensorData>> GetUpdates()
        {
            try
            {
                return _monitoringCore.GetSensorUpdates(HttpContext.User as User);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return BadRequest();
            }
        }

        [HttpGet("GetHistory")]
        public ActionResult<List<SensorHistoryData>> GetHistory([FromBody] GetSensorHistoryModel model)
        {
            try
            {
                return _monitoringCore.GetSensorHistory(HttpContext.User as User, model);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return BadRequest();
            }
        }
    }
}
