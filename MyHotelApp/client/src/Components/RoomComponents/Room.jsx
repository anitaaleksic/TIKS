import axios from 'axios';
import avatarRoom from '../../assets/RoomLogo.png';
 // stavi sliku sobe u assets
import EntityList from '../EntityList';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';


export default function Room() {
  const [refresh, setRefresh] = useState(false);

  const navigate = useNavigate();


  const handleInfo = (roomNumber) => {
  console.log('Navigating to room info with roomnumber:', roomNumber);
  navigate(`/room/info/${roomNumber}`);
};

  const handleEdit = (room) => {
    console.log('Navigating to edit guest with room number:', room);
    navigate(`/room/edit/${room}`);
  };

  const handleDelete = async (roomNumber) => {
    if (!window.confirm(`Are you sure you want to delete room ${roomNumber}?`)) return;
    try {
      await axios.delete(`/api/Room/DeleteRoom/${roomNumber}`);
      setRefresh(prev => !prev); // Trigger re-fetch
    } catch (err) {      
        if (err.response && err.response.data) {
          alert(err.response.data);
        } else {
          alert("Delete failed due to an unexpected error.");
        }
        console.error("Delete failed:", err);
      }
  };

  return (
    <EntityList
      addRoute="/addroom"
      fetchUrl="/api/Room/GetAllRooms"
      backgroundImage={avatarRoom}
      renderFields={room => (
        <>
          <p><strong>Room Number:</strong> {room.roomNumber}</p>
          <p><strong>Room Type ID:</strong> {room.roomTypeID}</p>
          <p><strong>Floor:</strong> {room.floor}</p>
          <p><strong>Available:</strong> {room.isAvailable ? 'Yes' : 'No'}</p>
        </>
      )}
      onEdit={handleEdit}
      onInfo={handleInfo}
      onDelete={handleDelete}
      idField="roomNumber"
      refreshTrigger={refresh}
    />
  );
}
