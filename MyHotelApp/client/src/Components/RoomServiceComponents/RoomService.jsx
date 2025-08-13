import axios from 'axios';
//import avatarRoomService from '../../assets/RoomServiceLogo.png';  
//import EntityList from '../EntityList';
import "../../css/ServiceTable.css";
import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';

export default function RoomService() {
  //const [refresh, setRefresh] = useState(false);
  const [roomServices, setRoomServices] = useState([]);

  const navigate = useNavigate(); 

  async function GetAllRoomServices(){
    const response = await axios.get("/api/RoomService/GetAllRoomServices");
    return response.data;
  }

  useEffect(() => {
    async function loadRoomServices() {
      const data = await GetAllRoomServices();
      setRoomServices(data);
    }
    loadRoomServices();
  }, [])

  const handleAdd = () => {
    navigate("/addroomservice");
  };

  const handleEdit = (itemName) => navigate(`/roomservice/edit/${encodeURIComponent(itemName)}`);
  //const handleInfo = (itemName) => navigate(`/roomservice/info/${encodeURIComponent(itemName)}`);
  

  return (
    <div className="entity-page-wrapper">
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add Room Service
        </button>
      </div>
      <div className='table-container'>
        <table id="roomServiceTable">
          <thead>
            <tr>
              <th>Room Services</th>
            </tr>
          </thead>
          <tbody>
            {roomServices.map((roomService) => (
              <tr key={roomService.RoomServiceID} onClick={() => handleEdit(roomService.itemName)}>
                <td className="menu-item">
                  <div className="name-desc">
                    <div className="item-name">{roomService.itemName}</div>
                    <div className="description">({roomService.description})</div>
                  </div>
                  <span className="dots"></span>
                  <span className="price">{roomService.itemPrice.toFixed(2)}$</span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>    
    </div>
  );
}
