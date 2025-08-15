import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function EditRoom() {
  const { roomNumber } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    roomNumber: '',
    roomTypeID: '',
    floor: ''//,
    //isAvailable: false
  });

  const [errorMessages, setErrorMessages] = useState([]);
  const [roomTypes, setRoomTypes] = useState([]);
  
  //const [refresh, setRefresh] = useState(false);

  async function GetAllRoomTypes() {
    const response = await axios.get("/api/RoomType/GetAllRoomTypes")
    return response.data;
  }



  useEffect(() => {
    axios.get(`/api/Room/GetRoom/${roomNumber}`)
      .then(res => setFormData(res.data))
      .catch(err => {
        console.error(err);
        alert('Failed to load room data.');
        navigate('/room');
      });
    async function loadRoomTypes() {
      const data = await GetAllRoomTypes();
      setRoomTypes(data);      
    }
    loadRoomTypes();
  }, [roomNumber, navigate]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const errors = [];
    if (!formData.roomTypeID) errors.push('Room Type ID is required.');
    if (formData.floor === '') errors.push('Floor is required.');

    if (errors.length > 0) {
      setErrorMessages(errors);
      return;
    }

    axios.put(`/api/Room/UpdateRoom/${roomNumber}`, formData)
      .then(() => {
        alert('Room updated successfully!');
        navigate('/room');
      })
      .catch(err => {
        console.error(err);
        setErrorMessages([err.response?.data?.message || 'Update failed.']);
      });
  };

  
  const handleDelete = async (roomNumber) => {
    if (!window.confirm(`Are you sure you want to delete room ${roomNumber}?`)) return;
    try {
      await axios.delete(`/api/Room/DeleteRoom/${roomNumber}`);
      //setRefresh(prev => !prev); // Trigger re-fetch
      alert('Room deleted successfully!');
      navigate("/room");
    } catch (err) {      
        if (err.response && err.response.data) {
          alert(err.response.data);
        } else {
          alert("Delete failed due to an unexpected error.");
        }
        console.error("Delete failed:", err);
      }
  };

  const handleExit = () => {
    navigate("/room");
  }

  return (
    <form className="extraservice-form" onSubmit={handleSubmit}>
      <button type="button" className="exit-button" onClick={handleExit}>
        x
      </button>
      <h2>Edit Room</h2>

      <div className="form-group">
        <label className="form-label">Room Number:</label>
        <input
          className="form-input"
          name="roomNumber"
          type="text"
          value={formData.roomNumber}
          disabled
        />
      </div>

      <div className="form-group">
        <label className="form-label">Room Type:</label>
        <select 
          name = "roomTypeID"
          className="form-input" 
          value={formData.roomTypeID}
          onChange={handleChange}>
          {roomTypes.map((rt) => (
            <option value={rt.roomTypeID}>{rt.type}</option>
          ))}
        </select>
        
      </div>

      <div className="form-group">
        <label className="form-label">Floor:</label>
        <input
          className="form-input"
          name="floor"
          type="number"
          value={formData.floor}
          disabled
        />
      </div>


      <button type="submit" className="form-button">Update Room</button>
      <button 
        type="button" 
        className="form-button delete" 
        onClick={(e) => {
          e.preventDefault(); 
          handleDelete(roomNumber);
        }}>
          Delete Room
        </button>


      {errorMessages.length > 0 && (
        <div style={{ color: 'red', marginTop: '1rem' }}>
          <ul>
            {errorMessages.map((msg, idx) => <li key={idx}>{msg}</li>)}
          </ul>
        </div>
      )}
    </form>
  );
}
