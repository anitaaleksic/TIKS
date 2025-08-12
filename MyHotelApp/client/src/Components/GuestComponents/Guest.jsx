import axios from 'axios';
//import avatarGuest from '../../assets/GuestLogo.png';
import { useNavigate } from 'react-router-dom';

//import EntityList from '../EntityList';
import "../../css/GuestTable.css";
import "../../css/App.css";
import { useEffect, useState } from 'react';

export default function Guest() {
  //const [refresh, setRefresh] = useState(false);
  const navigate = useNavigate();

  const [guests, setGuests] = useState([]);

  async function GetAllGuests(){
    const response = await axios.get("/api/Guest/GetAllGuests");
    return response.data;
  }

  useEffect(() => {
    async function loadGuests(){
      const data = await GetAllGuests();
      setGuests(data);
    }
    loadGuests();
  }, []);


  

  const handleAdd = () => {
    navigate("/addguest");
  };

  const handleEdit = (guest) => {
    console.log('Navigating to edit guest with jmbg:', guest);
    navigate(`/editguest/${guest}`);
  };

  // const handleInfo = (guestJmbg) => {
  // console.log('Navigating to guest info with jmbg:', guestJmbg);
  // navigate(`/guestinfo/${guestJmbg}`);
  // };

  // const handleDelete = async (jmbg) => {
  //   if (!window.confirm(`Are you sure you want to delete guest ${jmbg}?`)) return;
  //   try {
  //       console.log("Deleting JMBG:", jmbg, "Type:", typeof jmbg);

  //     await axios.delete(`/api/Guest/DeleteGuest/${jmbg}`);
  //     setRefresh(prev => !prev); // Trigger re-fetch
  //   } catch (err) {
  //     console.error("Delete failed:", err);
  //     alert("Delete failed");
  //   }
  // };

  return (
    
    <div className="entity-page-wrapper">
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add Guest
        </button>
      </div>
      <div className='table-container'>
        <table id="guestTable">
          <thead>
            <tr>
              <th>Full Name</th>
              <th>JMBG</th>
              <th>Phone Number</th>
            </tr>
          </thead>
          <tbody>
            {guests.map((guest) => (
              <tr key={guest.jmbg} onClick={() => handleEdit(guest.jmbg)}>
                <td>{guest.fullName}</td>
                <td>{guest.jmbg}</td>
                <td>{guest.phoneNumber}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>










    {/* <EntityList
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
      
    /> */}



    
    </div>
  );
}
