import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function InfoGuest() {
  const { jmbg } = useParams();
  const navigate = useNavigate();

  const [guest, setGuest] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios.get(`/api/Guest/GetGuestByJMBG/${jmbg}`)
      .then(res => setGuest(res.data))
      .catch(err => {
        console.error(err);
        setError('Failed to load guest data.');
      });
  }, [jmbg]);

  if (error) {
    return (
      <div className="guest-form" style={{ maxWidth: '400px', margin: '100px auto' }}>
        <p style={{ color: 'red' }}>{error}</p>
        <button className="form-button" onClick={() => navigate(-1)}>Back</button>
      </div>
    );
  }

  if (!guest) {
    return <p style={{ textAlign: 'center', marginTop: '2rem' }}>Loading...</p>;
  }

  return (
    <div className="guest-form">
      <h2>Guest Information</h2>

      <div className="form-group">
        <label className="form-label">Full Name:</label>
        <p>{guest.fullName}</p>
      </div>

      <div className="form-group">
        <label className="form-label">JMBG:</label>
        <p>{guest.jmbg}</p>
      </div>

      <div className="form-group">
        <label className="form-label">Phone Number:</label>
        <p>{guest.phoneNumber}</p>
      </div>

      <button className="form-button" onClick={() => navigate(-1)}>Back</button>
    </div>
  );
}
