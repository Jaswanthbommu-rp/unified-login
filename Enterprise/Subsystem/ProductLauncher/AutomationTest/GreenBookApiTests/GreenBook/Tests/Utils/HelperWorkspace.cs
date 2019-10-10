using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OsNextApiTestingFramework;
using OsNextApiTestingFramework.Controllers;
using RPNextAffordable.Models;

namespace RPNextAffordable.Utils
{
    class HelperWorkspace : TestBase
    {
        private WorkSpaceDetail _wsDet = new WorkSpaceDetail();
        private List<WorkSpaceDetail> _wsDetList = new List<WorkSpaceDetail>();
        private WorkspaceList _wsliReq = new WorkspaceList();
        private List<WorkspaceList> _wslReq = new List<WorkspaceList>();
        private List<Workspace> _wslRes = new List<Workspace>();

        private SaveHAPWorkspaces _wsReq = new SaveHAPWorkspaces();
        private SaveHAPWorkspaces _wsReqBef = new SaveHAPWorkspaces();

        private GetAllHAPWorkspaceSummaries _wsRes = new GetAllHAPWorkspaceSummaries();
        public SaveHAPWorkspaces TestWs { get; set; }

        /// <summary>
        /// GetWorkspace : Retrieves the current workspace object
        /// </summary>
        /// <returns>"GetAllHAPWorkspaceSummaries Object"</returns>
        public GetAllHAPWorkspaceSummaries GetWorkspace()
        {
            //Set up the api url 
            EndPointUrl = HostUrl + Properties["GetAffordableWorkspaces"];

            //Act
            var response = GetHttpWebResponse(EndPointUrl, AuthHeader, HttpVerb.GET);
            //Extratct json string value
            var responsevalue = getHttpWebResponseValue(response);
            Assert.IsTrue(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");

            var responseData =
                JsonConvert.DeserializeObject<GetAllHAPWorkspaceSummaries>(responsevalue);

            return responseData;
        }

        /// <summary>
        /// PutWorkspace : Execute and Assert the SaveHAPWorkspaces object converted to String 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>"SuccessResponse Object"</returns>
        public SuccessResponse PutWorkspace(string payload)
        {
            //Set up the api url 
            EndPointUrl = HostUrl + Properties["PutAffordableWorkspaces"];

            //Act
            var response = GetHttpWebResponse(EndPointUrl, AuthHeader, HttpVerb.PUT, payload);
            //Extratct json string value
            var responsevalue = getHttpWebResponseValue(response);
            Assert.IsTrue(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");

            var responseData = JsonConvert.DeserializeObject<SuccessResponse>(responsevalue);

            return responseData;
        }


        /// <summary>
        /// GetExistingUserWorkspace : Get the Existing User Workspace to frame the PUT Workspace Object
        /// </summary>
        /// <returns>"SaveHAPWorkspaces Object"</returns>
        public SaveHAPWorkspaces GetExistingUserWorkspace()
        {
            _wsReq = new SaveHAPWorkspaces();
            _wslReq = new List<WorkspaceList>();
            _wsRes = GetWorkspace();
            _wslRes = _wsRes.records;

            foreach (var resRec in _wslRes)
            {
                _wsliReq = new WorkspaceList();
                _wsDetList = new List<WorkSpaceDetail>();
                _wsliReq.activityUIID = resRec.activityUIID;
                _wsliReq.dirtyBit = resRec.dirtyBit;
                _wsliReq.guid = resRec.guid;
                _wsliReq.isActive = resRec.isActive;
                _wsliReq.productAreaID = resRec.productAreaID;
                _wsliReq.sequence = resRec.sequence;
                _wsliReq.title = resRec.title;
                _wsliReq.userWorkspaceID = resRec.userWorkspaceID;

                foreach (var det in resRec.details)
                {
                    _wsDet = new WorkSpaceDetail
                    {
                        metric = det.metric,
                        status = det.status,
                        description = det.description,
                        guid = det.guid
                    };
                    _wsDetList.Add(_wsDet);
                }
                _wsliReq.details = _wsDetList;
                _wslReq.Add(_wsliReq);
            }

            _wsReq.workspaceList = _wslReq;
            return _wsReq;
        }

        /// <summary>
        /// PutWorkSpaceAndExecute : Convert the SaveHAPWorkspaces to Json object and Execute
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public void PutWorkSpaceAndExecute(SaveHAPWorkspaces ws)
        {
            var payload = JsonConvert.SerializeObject(ws);
            var message = PutWorkspace(payload);

            Assert.IsTrue(message.messageId > 0);
            Assert.IsTrue(message.messageText == "Success");
        }

        /// <summary>
        /// GetWorkspaceUserAllActive : Gets the Existing User Workspace and builds/returns the SaveHAPWorkspaces with all active
        /// </summary>
        /// <returns>"SaveHAPWorkspaces Object"</returns>
        public SaveHAPWorkspaces GetWorkspaceUserAllActive()
        {
            _wsReq = new SaveHAPWorkspaces();
            // Get Existing Workspace
            _wsReq = GetExistingUserWorkspace();

            var i = 1;
            // Set all the workspaces to Active and Execute
            foreach (var ws in _wsReq.workspaceList)
            {
                ws.isActive = true;
                ws.sequence = i;
                ws.dirtyBit = true;
                i++;
            }
            return _wsReq;
        }

        /// <summary>
        /// TestGetWorkspaceUserAllActiveVerify : Gets the Existing User Workspace asserts for all the workspaces are Active
        /// </summary>
        /// <returns></returns>
        public void TestGetWorkspaceUserAllActiveVerify()
        {
            // Get the workspaces and Assert
            _wsRes = GetWorkspace();
            _wslRes = _wsRes.records;
            foreach (var ws in _wslRes)
            {
                Assert.IsTrue(ws.isActive);
            }
        }

        /// <summary>
        /// GetWorkspaceUserAllActive : Gets the Existing User Workspace and builds/returns the SaveHAPWorkspaces with All Inactive
        /// </summary>
        /// <returns>"SaveHAPWorkspaces Object"</returns>
        public SaveHAPWorkspaces TestGetWorkspaceUserAllInactive()
        {
            // Get Existing Workspace
            _wsReq = GetExistingUserWorkspace();

            // Set all the workspaces to Inactive and Execute
            foreach (var ws in _wsReq.workspaceList)
            {
                ws.isActive = false;
                ws.sequence = 0;
                ws.dirtyBit = true;
            }
            return _wsReq;
        }

        /// <summary>
        /// TestGetWorkspaceUserAllInactiveVerify : Gets the Existing User Workspace asserts for all the workspaces are Inactive
        /// </summary>
        /// <returns></returns>
        public void TestGetWorkspaceUserAllInactiveVerify()
        {
            // Get the workspaces and Assert
            _wsRes = GetWorkspace();
            _wslRes = _wsRes.records;
            foreach (var ws in _wslRes)
            {
                Assert.IsFalse(ws.isActive);
            }
        }

        /// <summary>
        /// PutWorkspaceUserMoveAndExecute : Move the Workspace from current sequence to new sequence and execute
        /// </summary>
        /// <param name="curSeq"></param>
        /// <param name="newSeq"></param>
        /// <returns></returns>
        public void PutWorkspaceUserMoveAndExecute(int curSeq, int newSeq)
        {
            _wsReqBef = GetExistingUserWorkspace();
            _wsReq = GetExistingUserWorkspace();
            var len = _wsReq.workspaceList.Count;

            foreach (var wsreqItem in _wsReq.workspaceList)
            {
                if (curSeq <= len && curSeq > 0 && newSeq <= len && newSeq > 0)
                {
                    if (curSeq < newSeq)
                    {
                        if (wsreqItem.sequence < curSeq)
                        {
                        }
                        else if (wsreqItem.sequence == curSeq)
                        {
                            wsreqItem.sequence = newSeq;
                        }
                        else if (wsreqItem.sequence > curSeq && wsreqItem.sequence <= newSeq)
                        {
                            wsreqItem.sequence = wsreqItem.sequence - 1;
                        }
                        wsreqItem.dirtyBit = true;
                    }
                    else
                    {
                        if (wsreqItem.sequence > curSeq)
                        {
                        }
                        else if (wsreqItem.sequence == curSeq)
                        {
                            wsreqItem.sequence = newSeq;
                        }
                        else if (wsreqItem.sequence < curSeq && wsreqItem.sequence >= newSeq)
                        {
                            wsreqItem.sequence = wsreqItem.sequence + 1;
                        }
                        wsreqItem.dirtyBit = true;
                    }
                }
            }
            PutWorkSpaceAndExecute(_wsReq);
        }
        
        /// <summary>
        /// TestGetWorkspaceUserMoveVerify : Validate the Workspace sequence after changing the workspace sequences
        /// </summary>
        /// <param name="curSeq"></param>
        /// <param name="newSeq"></param>
        /// <returns></returns>
        public void TestGetWorkspaceUserMoveVerify(int curSeq, int newSeq)
        {
            _wsReq = GetExistingUserWorkspace();
            var len = _wsReq.workspaceList.Count;

            var i = 0;
            foreach (var wsreqItem in _wsReqBef.workspaceList)
            {
                if (curSeq <= len && curSeq > 0 && newSeq <= len && newSeq > 0)
                {
                    if (curSeq < newSeq)
                    {
                        if (wsreqItem.sequence < curSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[i].title);
                        }
                        else if (wsreqItem.sequence == curSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[newSeq - 1].title);
                            //wsreqItem.sequence = newSeq;
                        }
                        else if (wsreqItem.sequence > curSeq && wsreqItem.sequence <= newSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[wsreqItem.sequence - 1 - 1].title);

                            //wsreqItem.sequence = wsreqItem.sequence - 1;
                        }
                        else
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[i].title);
                        }
                    }
                    else
                    {
                        if (wsreqItem.sequence > curSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[i].title);
                        }
                        else if (wsreqItem.sequence == curSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[newSeq - 1].title);
                            //  wsreqItem.sequence = newSeq;
                        }
                        else if (wsreqItem.sequence < curSeq && wsreqItem.sequence >= newSeq)
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[wsreqItem.sequence + 1 - 1].title);
                            //wsreqItem.sequence = wsreqItem.sequence + 1;
                        }
                        else
                        {
                            Assert.IsTrue(wsreqItem.title == _wsReq.workspaceList[i].title);
                        }
                    }
                }
                i = i + 1;
            }
        }
    }
}
