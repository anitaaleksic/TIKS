import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

export default function InfoRoom() {
  const { roomNumber } = useParams();
  const navigate = useNavigate();

  const [room, setRoom] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    axios.get(`/api/Room/GetRoom/${roomNumber}`)
      .then(res => setRoom(res.data))
      .catch(err => {
        console.error(err);
        setError('Failed to load room data.');
      });
  }, [roomNumber]);

  if (error) {
    return (
      <div>
        <p style={{ color: 'red' }}>{error}</p>
        <button onClick={() => navigate(-1)}>Back</button>
      </div>
    );
  }

  if (!room) {
    return <p>Loading...</p>;
  }

  return (
    <div className="room-form">
      <h2>Room Information</h2>
      <p><strong>Room Number:</strong> {room.roomNumber}</p>
      <p><strong>Room Type ID:</strong> {room.roomTypeID}</p>
      <p><strong>Floor:</strong> {room.floor}</p>
      <button className="form-button" onClick={() => navigate(-1)}>Back</button>
    </div>
  );
}
