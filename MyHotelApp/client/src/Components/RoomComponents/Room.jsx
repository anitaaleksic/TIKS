import axios from 'axios';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "../../css/Room.css";
import stars from '../../assets/hotel-stars.jpg';


export default function Room() {
  

  const navigate = useNavigate();

  const [rooms, setRooms] = useState([]);

  async function GetAllRooms() {
    const response = await axios.get("/api/Room/GetAllRooms");
    return response.data;
  }

  useEffect(() => {
    async function loadRooms() {
      const data = await GetAllRooms();
      setRooms(data);
    }
    loadRooms();

  }, []);
  
  const floors = {};

  for(const room of rooms){
    

    if(!floors[room.floor]){
      floors[room.floor] = [];
    }
    
    floors[room.floor].push(room);
  }


  const handleAdd = () => {
    navigate("/addroom");
  };

  const handleEdit = (room) => {
    navigate(`/room/edit/${room}`);
  };

  return (
    <div className='entity-page-wrapper'>
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add Room
        </button>
      </div>
      <img src={stars} className='stars'></img>
      <table className='hotel'>
        <tbody>
          {Object.entries(floors).map(([floorNumber, floorRooms]) => (
            <tr className='hotel-floor' key={floorNumber}>
              {floorRooms.map((room) => (
                <td className='room-container' key={room.roomNumber} onClick={() => handleEdit(room.roomNumber)} data-roomNumber={room.roomNumber}>
                  <div>Room {room.roomNumber}</div>
                  <div>type: {room.roomType.type}</div>
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
        
    </div>
  );
}
