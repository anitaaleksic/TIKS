import axios from 'axios';
import avatarRoomService from '../../assets/RoomServiceLogo.png';  
import EntityList from '../EntityList';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
export default function RoomService() {
  const [refresh, setRefresh] = useState(false);

  const navigate = useNavigate(); 

  const handleEdit = (itemName) => navigate(`/roomservice/edit/${encodeURIComponent(itemName)}`);
  const handleInfo = (itemName) => navigate(`/roomservice/info/${encodeURIComponent(itemName)}`);
  const handleDelete = async (id) => {
    if (!window.confirm(`Are you sure you want to delete room service ${id}?`)) return;
    try {
      await axios.delete(`/api/RoomService/DeleteRoomService/${id}`);
      setRefresh(prev => !prev); 
    } catch (err) {
      console.error("Delete failed:", err);
      alert("Delete failed");
    }
  };

  return (
    <EntityList
      addRoute="/addroomservice"
      fetchUrl="/api/RoomService/GetAllRoomServices"
      backgroundImage={avatarRoomService}
      renderFields={service => (
        <>
          <p><strong>Item Name:</strong> {service.itemName}</p>
          <p><strong>Price:</strong> ${service.itemPrice.toFixed(2)}</p>
          <p><strong>Description:</strong> {service.description}</p>
        </>
      )}
      onEdit={handleEdit}
      onInfo={handleInfo}
      onDelete={handleDelete}
      idField="itemName"
      refreshTrigger={refresh}
    />
  );
}
