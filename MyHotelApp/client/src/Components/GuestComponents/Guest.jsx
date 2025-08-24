import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import "../../css/GuestTable.css";
import "../../css/App.css";
import { useEffect, useState } from 'react';

export default function Guest() {
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
              <tr key={guest.jmbg} onClick={() => handleEdit(guest.jmbg)} data-jmbg={guest.jmbg}>
                <td className='td-guest'>{guest.fullName}</td>
                <td className='td-guest'>{guest.jmbg}</td>
                <td className='td-guest'>{guest.phoneNumber}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>    
    </div>
  );
}
