import unittest
import numpy as np
from matplotlib.pyplot import imread

import poravnava

def load_img_uint8(path):
    img = imread(path)
    if img.ndim==3 and img.shape[2] == 4:
        img = img[:, :, :3].copy()
    if img.dtype != np.uint8:
        img = np.uint8(img*255)

    return img

class TestPoravnava(unittest.TestCase):
    def test_1_resi_procrustes_1_primer(self):
        tocke_A = np.array(
           [[ 0.05,  1.64],
            [-1.23,  0.56],
            [ 0.69,  0.7 ],
            [-0.26, -2.36],
            [-1.2 , -0.15],
            [-0.59,  0.4 ],
            [-0.84,  0.83],
            [-0.48,  1.22],
            [ 0.71, -0.56],
            [-0.5 ,  0.58]])
        tocke_B = np.array(
           [[ 0.63,  2.18],
            [-0.56,  0.99],
            [ 1.35,  1.3 ],
            [ 0.66, -1.83],
            [-0.46,  0.29],
            [ 0.1 ,  0.89],
            [-0.19,  1.3 ],
            [ 0.14,  1.72],
            [ 1.47,  0.04],
            [ 0.17,  1.08]])
        tr_ref = np.array([[0.718, 0.541]])
        R_ref = np.array([[ 0.996,  0.086],
                       [-0.086,  0.996]])

        R_est, tr_est = poravnava.resi_procrustes(tocke_A, tocke_B)
        np.testing.assert_array_almost_equal(R_est, R_ref, 2,
            err_msg='ocenjena rotacija ni dovolj podobna pricakvani')
        np.testing.assert_array_almost_equal(tr_est, tr_ref, 2,
            err_msg='ocenjena translacija ni dovolj podobna pricakovani')

    def test_2_resi_procrustes_2_primer(self):
        tocke_A = np.array(
           [[ 0.05,  1.64],
            [-1.23,  0.56],
            [ 0.69,  0.7 ],
            [-0.26, -2.36],
            [-1.2 , -0.15],
            [-0.59,  0.4 ],
            [-0.84,  0.83],
            [-0.48,  1.22],
            [ 0.71, -0.56],
            [-0.5 ,  0.58]])
        tocke_B = np.array(
           [[-0.63,  2.18],
            [ 0.56,  0.99],
            [-1.35,  1.3 ],
            [-0.66, -1.83],
            [ 0.46,  0.29],
            [-0.1 ,  0.89],
            [ 0.19,  1.3 ],
            [-0.14,  1.72],
            [-1.47,  0.04],
            [-0.17,  1.08]])

        R_ref = np.array([[ 0.975, -0.221],
                       [ 0.221,  0.975]])
        tr_ref = np.array([[-0.038,  0.436]])

        R_est, tr_est = poravnava.resi_procrustes(tocke_A, tocke_B)

        np.testing.assert_array_almost_equal(R_est, R_ref, 2,
            err_msg='ocenjena rotacija ni dovolj podobna pricakovani, manjka korekcija rotacijske matrike?')
        np.testing.assert_array_almost_equal(tr_est, tr_ref, 2,
            err_msg='ocenjena translacija ni dovolj podobna pricakovani')

    def test_3_oceni_transformacijo_icp_primer_1(self):
        slika_A = load_img_uint8('primer_preprost_1.png')[::2, ::2]
        slika_B = load_img_uint8('primer_preprost_2.png')[::2, ::2]

        R_ref = np.array([[ 0.673, -0.739],
                          [ 0.739,  0.673]])
        tr_ref = np.array([[10.219, 42.585]])

        R_est, tr_est = poravnava.oceni_transformacijo_ICP(slika_A, slika_B, max_itt=30)

        np.testing.assert_array_almost_equal(R_est, R_ref, 2,
            err_msg='ocenjena rotacija ni dovolj podobna pricakovani')
        np.testing.assert_array_almost_equal(tr_est, tr_ref, 2,
            err_msg='ocenjena translacija ni dovolj podobna pricakovani')

    def test_4_oceni_transformacijo_icp_primer_2_barve(self):
        slika_A = load_img_uint8('tests/primer_preprost_1.png')[::2, ::2]
        slika_B = load_img_uint8('tests/primer_preprost_2.png')[::2, ::2]

        R_ref = np.array([[ 0.303,  0.952],
                          [-0.952,  0.303]])
        tr_ref = np.array([[73.273, 0.854]])

        R_est, tr_est = poravnava.oceni_transformacijo_ICP(slika_A, slika_B, barve=True, max_itt=30)

        np.testing.assert_array_almost_equal(R_est, R_ref, 2,
            err_msg='ocenjena rotacija ni dovolj podobna pricakovani')
        np.testing.assert_array_almost_equal(tr_est, tr_ref, 2,
            err_msg='ocenjena translacija ni dovolj podobna pricakovani')

    def test_5_oceni_transformacijo_icp_primer_3_ransac(self):
        slika_A = load_img_uint8('primer_zahteven_3.png')[::2, ::2]
        slika_B = load_img_uint8('primer_zahteven_4.png')[::2, ::2]

        R_ref = np.array([[ 0.961, -0.273],
                          [ 0.273,  0.961]])
        tr_ref = np.array([[-3.165, 12.577]])

        R_est, tr_est = poravnava.oceni_transformacijo_ICP(slika_A, slika_B, ransac=True, max_itt=30)

        np.testing.assert_array_almost_equal(R_est, R_ref, 2,
            err_msg='ocenjena rotacija ni dovolj podobna pricakovani')
        np.testing.assert_array_almost_equal(tr_est, tr_ref, 2,
            err_msg='ocenjena translacija ni dovolj podobna pricakovani')

if __name__ == '__main__':
    unittest.main(verbosity=2)
