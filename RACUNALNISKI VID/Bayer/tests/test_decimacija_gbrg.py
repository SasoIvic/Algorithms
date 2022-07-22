import bayer
import unittest
import numpy as np

class Test(unittest.TestCase):
    def test_decimacija_gbrg(self):
        podatki = np.array(
            [[110, 150, 220, 230, 110, 180],
             [120, 160, 210, 240, 150, 140],
             [130, 170, 200, 250, 120, 170],
             [140, 180, 190, 250, 160, 130]], dtype=np.uint8)

        resitev_gbrg = np.array(
            [[[120, (160+110)//2, 150], 
              [210, (240+220)//2, 230], 
              [150, (140+110)//2, 180]], 
             [[140, (180+130)//2, 170], 
              [190, (250+200)//2, 250], 
              [160, (130+120)//2, 170]]], dtype=np.uint8)

        rezultat = bayer.bayer_v_rgb(podatki, 'GBRG')
        self.assertIsInstance(rezultat, np.ndarray)
        self.assertEqual(rezultat.dtype, np.uint8)
        np.testing.assert_array_equal(rezultat, resitev_gbrg)

if __name__ == '__main__':
    unittest.main(verbosity=2)
