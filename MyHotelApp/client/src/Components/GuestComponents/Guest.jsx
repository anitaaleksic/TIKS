import axios from 'axios';
import avatarGuest from '../../assets/GuestLogo.png';
import { useNavigate } from 'react-router-dom';

import EntityList from '../EntityList';
import {  useState } from 'react';

export default function Guest() {
  const [refresh, setRefresh] = useState(false);


  const navigate = useNavigate();

  const handleEdit = (guest) => {
    console.log('Navigating to edit guest with jmbg:', guest);
    navigate(`/editguest/${guest}`);
  };

  const handleInfo = (guestJmbg) => {
  console.log('Navigating to guest info with jmbg:', guestJmbg);
  navigate(`/guestinfo/${guestJmbg}`);
};

  const handleDelete = async (jmbg) => {
    if (!window.confirm(`Are you sure you want to delete guest ${jmbg}?`)) return;
    try {
        console.log("Deleting JMBG:", jmbg, "Type:", typeof jmbg);

      await axios.delete(`/api/Guest/DeleteGuest/${jmbg}`);
      setRefresh(prev => !prev); // Trigger re-fetch
    } catch (err) {
      console.error("Delete failed:", err);
      alert("Delete failed");
    }
  };

  return (
    <EntityList
      addRoute="/addguest"
      fetchUrl="/api/Guest/GetAllGuests"
      backgroundImage={avatarGuest}
      renderFields={guest => (
        <>
          <p><strong>Full Name:</strong> {guest.fullName}</p>
          <p><strong>JMBG:</strong> {guest.jmbg}</p>
          <p><strong>Phone:</strong> {guest.phoneNumber}</p>
        </>
      )}
      onEdit={handleEdit}
      onInfo={handleInfo}
      onDelete={handleDelete}
      idField="jmbg"
      refreshTrigger={refresh} 
    />
  );
}
