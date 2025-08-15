import { useParams, useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import axios from 'axios';

export default function EditRoomService() {
  const { itemName } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    roomServiceID: '',
    itemName: '',
    itemPrice: '',
    description: ''
  });

  const [originalName, setOriginalName] = useState('');
  const [errorMessages, setErrorMessages] = useState([]);
  //const [refresh, setRefresh] = useState(false);

  useEffect(() => {
    axios.get(`/api/RoomService/GetRoomServiceByName/${itemName}`)
      .then(res => {
        setFormData(res.data);
        setOriginalName(res.data.itemName);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to load room service data.');
        navigate('/roomservice');
      });
  }, [itemName]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleDelete = async (id) => {
    if (!window.confirm(`Are you sure you want to delete room service ${id}?`)) return;
    try {
      await axios.delete(`/api/RoomService/DeleteRoomService/${id}`);
      alert('Room service deleted successfully.');
      //setRefresh(prev => !prev); 
      navigate("/roomservice");
    } catch (err) {
      console.error("Delete failed:", err);
      alert("Delete failed");
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const errors = [];
    if (!formData.itemName || formData.itemName.length > 100) {
      errors.push('Item name is required and must be less than 100 characters.');
    }
    if (!formData.itemPrice || isNaN(formData.itemPrice)) {
      errors.push('Valid price is required.');
    }

    if (errors.length > 0) {
      setErrorMessages(errors);
      return;
    }

    axios.put(`/api/RoomService/UpdateRoomServiceByName/${originalName}`, formData)
      .then(() => {
        alert('Room service updated successfully!');
        navigate('/roomservice');
      })
      .catch(err => {
        console.error(err);
        setErrorMessages([err.response?.data?.message || 'Update failed.']);
      });
  };

  const handleExit = () => {
    navigate("/roomservice");
  }

  return (
    <form className="extraservice-form" onSubmit={handleSubmit}>
      <button type="button" className="exit-button" onClick={handleExit}>
        x
      </button>
      <h2>Edit Room Service</h2>

      <div className="form-group">
        <label className="form-label">Item Name:</label>
        <input
          className="form-input"
          name="itemName"
          type="text"
          value={formData.itemName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Price:</label>
        <input
          className="form-input"
          name="itemPrice"
          type="number"
          step="0.01"
          value={formData.itemPrice}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">Description:</label>
        <textarea
          className="form-input"
          name="description"
          value={formData.description}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Update Room Service</button>
      <button 
        type="button" 
        className="form-button delete" 
        onClick={(e) => {
          e.preventDefault(); 
          handleDelete(formData.roomServiceID);
        }}>
          Delete Room Service
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
