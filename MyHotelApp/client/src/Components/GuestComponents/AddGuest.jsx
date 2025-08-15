import { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export default function AddGuest() {
  const [formData, setFormData] = useState({
    fullName: '',
    jmbg: '',
    phoneNumber: ''
  });

  const [errorMessages, setErrorMessages] = useState([]);
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData(prev => ({
      ...prev,
      [e.target.name]: e.target.value
    }));
  };

  const formatErrors = (errorsObj) => {
    let messages = [];
    for (const field in errorsObj) {
      const errors = errorsObj[field];
      messages.push(`${field}: ${errors.join(', ')}`);
    }
    return messages;
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    //validacije
    if(!formData.fullName || formData.fullName.length > 100) {
      setErrorMessages(['Full name is required and cannot exceed 100 characters.']);
      return;
    }

    if(!formData.jmbg || formData.jmbg.length !== 13) {
      setErrorMessages(['JMBG is required and must be 13 characters long.']);
      return;
    }


    axios.post('/api/Guest/CreateGuest', formData)
      .then(() => {
        alert('Guest added successfully!');
        setFormData({ fullName: '', jmbg: '', phoneNumber: ''});
        setErrorMessages([]);
        navigate("/guest");
      })
      .catch(err => {
        console.error('Error:', err.response || err);
        if (err.response?.data?.errors) {
          setErrorMessages(formatErrors(err.response.data.errors));
        } else {
          setErrorMessages([err.response?.data?.message || err.message]);
        }
      });

  };

  const handleExit = () => {
    navigate("/guest");
  }

  return (
    <form className="guest-form" onSubmit={handleSubmit}>
      <div className="form-group">
        <button type="button" className="exit-button" onClick={handleExit}>
        x
      </button>
        <label className="form-label" htmlFor="fullName">Full Name:</label>
        <input
          type="text"
          id="fullName"
          name="fullName"
          className="form-input"
          value={formData.fullName}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label" htmlFor="jmbg">JMBG:</label>
        <input
          type="text"
          id="jmbg"
          name="jmbg"
          className="form-input"
          value={formData.jmbg}
          onChange={handleChange}
        />
      </div>

      <div className="form-group">
        <label className="form-label" htmlFor="phoneNumber">Phone Number:</label>
        <input
          type="text"
          id="phoneNumber"
          name="phoneNumber"
          className="form-input"
          placeholder="+381..."
          value={formData.phoneNumber}
          onChange={handleChange}
        />
      </div>

      <button type="submit" className="form-button">Add guest</button>

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
