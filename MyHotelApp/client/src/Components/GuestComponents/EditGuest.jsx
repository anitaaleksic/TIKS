import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function EditGuest() {
  const { jmbg } = useParams();
  const navigate = useNavigate();
  //const [refresh, setRefresh] = useState(false);

  const [formData, setFormData] = useState({
    fullName: '',
    jmbg: '',
    phoneNumber: ''
  });

  const [errorMessages, setErrorMessages] = useState([]);

  useEffect(() => {
    axios.get(`/api/Guest/GetGuestByJMBG/${jmbg}`)
      .then(res => {
        setFormData(res.data);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to load guest data.');
      });
  }, [jmbg]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleDelete = async (jmbg) => {
    if (!window.confirm(`Are you sure you want to delete guest ${jmbg}?`)) return;
    try {
      console.log("Deleting JMBG:", jmbg, "Type:", typeof jmbg);

      await axios.delete(`/api/Guest/DeleteGuest/${jmbg}`);
      //setRefresh(prev => !prev); // Trigger re-fetch
      alert('Guest deleted successfully!');
      navigate("/guest");
    } catch (err) {
      // If backend sends a response with a message, show it
      if (err.response && err.response.data) {
        alert(err.response.data);
      } else {
        alert("Delete failed due to an unexpected error.");
      }
      console.error("Delete failed:", err);
    }

  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Validacija
    const errors = [];
    if (!formData.fullName || formData.fullName.length > 100) {
      errors.push('Full name is required and must be less than 100 characters.');
    }

    if (!formData.jmbg || formData.jmbg.length !== 13) {
      errors.push('JMBG is required and must be exactly 13 digits.');
    }

    if (!formData.phoneNumber || formData.phoneNumber.length > 20) {
      errors.push('Phone number is required and must be less than 20 characters.');
    }

    if (errors.length > 0) {
      setErrorMessages(errors);
      return;
    }

    try{
      await axios.put(`/api/Guest/UpdateGuest/${jmbg}`, formData)
      alert('Guest updated successfully!');
      navigate('/guest');
    }
    catch(err) {
      console.error('Error:', err.response || err);
        if (err.response?.data?.errors) {
          const messages = [];
          for (const field in err.response.data.errors) {
            messages.push(`${field}: ${err.response.data.errors[field].join(', ')}`);
          }
          setErrorMessages(messages);
        } else {
          setErrorMessages([err.response?.data?.message || err.message]);
        }
    }
  };

  return (
    <form className="guest-form" onSubmit={handleSubmit}>
      <h2>Edit Guest</h2>

      <div className="form-group">
        <label className="form-label">Full Name:</label>
        <input
          className="form-input"
          name="fullName"
          type="text"
          value={formData.fullName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label">JMBG:</label>
        <input
          className="form-input"
          name="jmbg"
          type="text"
          value={formData.jmbg}
          onChange={handleChange}
          disabled // JMBG ne treba menjati
        />
      </div>

      <div className="form-group">
        <label className="form-label">Phone Number:</label>
        <input
          className="form-input"
          name="phoneNumber"
          type="text"
          value={formData.phoneNumber}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Update Guest</button>
      <button 
        type="button" 
        className="form-button delete" 
        onClick={(e) => {
          e.preventDefault(); 
          handleDelete(jmbg);
        }}>
          Delete Guest
        </button>

      {errorMessages.length > 0 && (
        <div className="error-messages" style={{ color: 'red', marginTop: '1rem' }}>
          <h4>Errors:</h4>
          <ul>
            {errorMessages.map((msg, idx) => (
              <li key={idx}>{msg}</li>
            ))}
          </ul>
        </div>
      )}
    </form>
  );
}
