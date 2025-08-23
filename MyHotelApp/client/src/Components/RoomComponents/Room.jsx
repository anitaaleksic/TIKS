import axios from 'axios';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import "../../css/Room.css";
import stars from '../../assets/hotel-stars.jpg';


export default function Room() {
  

  const navigate = useNavigate();

  const [rooms, setRooms] = useState([]);
  //const [roomTypes, setRoomTypes] = useState([]);

  async function GetAllRooms() {
    const response = await axios.get("/api/Room/GetAllRooms");
    return response.data;
  }

  // async function GetAllRoomTypes() {
  //   const response = await axios.get("/api/RoomType/GetAllRoomTypes");
  //   return response.data;
  // }

  // useEffect(() => {
  //   async function loadData() {
  //   const [roomsData, roomTypesData] = await Promise.all([
  //       GetAllRooms(),
  //       GetAllRoomTypes()
  //     ]);
  //     setRooms(roomsData);
  //     setRoomTypes(roomTypesData);
  //   }
  //   loadData();

  // }, []);

  useEffect(() => {
    async function loadRooms() {
      const data = await GetAllRooms();
      setRooms(data);
    }
    loadRooms();

  }, []);

  
  // const roomTypeMap = {};
  // for (const type of roomTypes) {
  //   roomTypeMap[type.roomTypeID] = type.type;
  // }
  
  const floors = {};

  // for(const room of rooms){
  //   const typeName = roomTypeMap[room.roomTypeID];

  //   if(!floors[room.floor]){
  //     floors[room.floor] = [];
  //   }
    
  //   floors[room.floor].push({
  //     ...room,
  //     typeName
  //   });
  // }

  for(const room of rooms){
    

    if(!floors[room.floor]){
      floors[room.floor] = [];
    }
    
    floors[room.floor].push(room);
  }


  const handleAdd = () => {
    navigate("/addroom");
  };

  // const handleInfo = (roomNumber) => {
  // console.log('Navigating to room info with roomnumber:', roomNumber);
  // navigate(`/room/info/${roomNumber}`);
  // };


  const handleEdit = (room) => {
    console.log('Navigating to edit guest with room number:', room);
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
                  {/* <div>type: {room.typeName}</div> */}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
        
    </div>
  );
}
